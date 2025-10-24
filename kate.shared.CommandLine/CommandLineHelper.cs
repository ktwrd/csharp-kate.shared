/*
   Copyright 2022-2025 Kate Ward <kate@dariox.club>

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using ParseResult = System.CommandLine.ParseResult;

namespace kate.shared.CommandLine
{
    /// <summary>
    /// Helper Methods for generating commands.
    /// </summary>
    public static class CommandLineHelper
    {
        /// <summary>
        /// Generate an instance of <see cref="Command"/>
        /// </summary>
        /// <typeparam name="TOptions">
        /// Type of the options to deserialize.</typeparam>
        /// <param name="commandName">
        /// Name of the command.</param>
        /// <param name="commandHelpText">
        /// Help text for the command</param>
        /// <param name="handler">
        /// Handler to be called when the generated command was ran.</param>
        /// <returns>Instance of <see cref="Command"/></returns>
        public static Command GenerateCommand<TOptions>(string commandName, string commandHelpText, Func<TOptions, Task> handler)
            where TOptions : class, new()
        {
            return GenerateCommand(typeof(TOptions), commandName, commandHelpText, (obj) =>
            {
                if (obj == null)
                {
                    return handler(null);
                }
                if (!(obj is TOptions opts))
                {
                    throw new ArgumentException($"Not an instance of {typeof(TOptions)}", nameof(obj));
                }
                return handler(opts);
            });
        }

        /// <summary>
        /// Generate an instance of <see cref="Command"/>
        /// </summary>
        /// <typeparam name="TOptions">
        /// Type of the options to deserialize.</typeparam>
        /// <param name="commandName">
        /// Name of the command.</param>
        /// <param name="commandHelpText">
        /// Help text for the command</param>
        /// <param name="handler">
        /// Handler to be called when the generated command was ran.</param>
        /// <returns>Instance of <see cref="Command"/></returns>
        public static Command GenerateCommand<TOptions>(string commandName, string commandHelpText, Func<TOptions, ParseResult, Task> handler)
            where TOptions : class, new()
        {
            return GenerateCommand(typeof(TOptions), commandName, commandHelpText, (obj, ctx) =>
            {
                if (obj == null)
                {
                    return handler(null, ctx);
                }
                else if (obj is TOptions opts)
                {
                    return handler(opts, ctx);
                }
                throw new ArgumentException($"Not an instance of {typeof(TOptions)}", nameof(obj));
            });
        }

        /// <summary>
        /// Generate an instance of <see cref="Command"/>
        /// </summary>
        /// <param name="optionsType">
        /// Type of the options to deserialize.</param>
        /// <param name="commandName">
        /// Name of the command.</param>
        /// <param name="commandHelpText">
        /// Help text for the command</param>
        /// <param name="handler">
        /// Handler to be called when the generated command was ran.</param>
        /// <returns>Instance of <see cref="Command"/></returns>
        public static Command GenerateCommand(
            Type optionsType,
            string commandName,
            string commandHelpText,
            Func<object, ParseResult, Task> handler)
        {
            var argumentPropertyMap = new Dictionary<string, (object, ActionParameterAttribute)>();
            var optionsProps = optionsType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            var registeredParameterAliases = new List<(PropertyInfo Property, string Name, int? AliasAttributeIndex)>();
            string BuildAlias(ActionParameterAliasAttribute a)
            {
                if (a.Kind == ActionParameterAliasKind.DoubleDash)
                {
                    return "--" + a.Alias.Trim();
                }
                else if (a.Kind == ActionParameterAliasKind.SingleDash)
                {
                    return "-" + a.Alias.Trim();
                }
                else if (a.Kind == ActionParameterAliasKind.Custom && a.CustomPrefix != null)
                {
                    return a.CustomPrefix + a.Alias.Trim();
                }
                else
                {
                    return a.Alias.Trim();
                }
            }
            string BuildName(ActionParameterAttribute a)
            {
                var name = a.Name.Trim().Replace(' ', '-');
                if (name.StartsWith("--") == false)
                    name = $"--{name}";
                return name;
            }
            string BuildShortName(ActionParameterAttribute a)
            {
                var shortName = a.ShortNameAlias.Trim();
                if (!shortName.StartsWith("-"))
                {
                    shortName = "-" + shortName;
                }
                return shortName;
            }
            foreach (var prop in optionsProps)
            {
                var actionParamAttr = prop.GetCustomAttribute<ActionParameterAttribute>();
                if (actionParamAttr == null) continue;
                var actionParamName = BuildName(actionParamAttr);
                registeredParameterAliases.Add((prop, actionParamName, null));

                if (registeredParameterAliases.Any(e => e.Property != prop && e.Name == actionParamName))
                {
                    throw new InvalidOperationException($"An argument called \"{actionParamName}\" in property {prop.Name} already exists on {optionsType.Namespace}.{optionsType.Name}");
                }

                if (!string.IsNullOrEmpty(actionParamAttr.ShortNameAlias))
                {
                    var shortNameAlias = BuildShortName(actionParamAttr);
                    registeredParameterAliases.Add((prop, shortNameAlias, null));
                    if (registeredParameterAliases.Any(e => e.Property != prop && e.Name == shortNameAlias))
                    {
                        throw new InvalidOperationException($"An argument alias called \"{shortNameAlias}\" in property {prop.Name} already exists on {optionsType.Namespace}.{optionsType.Name}");
                    }
                }

                var aliasAttrArr = prop.GetCustomAttributes<ActionParameterAliasAttribute>().ToArray();
                for (int i = 0; i < aliasAttrArr.Length; i++)
                {
                    var aliasAttr = aliasAttrArr[i];
                    if (string.IsNullOrEmpty(aliasAttr.Alias))
                    {
                        throw new InvalidOperationException($"Alias cannot be empty for {nameof(ActionParameterAliasAttribute)}[{i}] on property {prop.Name} in class {optionsType.Namespace}.{optionsType.Name}");
                    }
                    var a = BuildAlias(aliasAttr);
                    registeredParameterAliases.Add((prop, a, i));
                    if (registeredParameterAliases.Any(e => e.Property != prop && e.Name == a))
                    {
                        throw new InvalidOperationException($"An argument alias called \"{a}\" in property {prop.Name} already exists on {optionsType.Namespace}.{optionsType.Name}");
                    }
                }
            }

            void SetDefaultValue(
                PropertyInfo prop,
                Type argumentInstanceType,
                object argumentInstance)
            {
                var defaultValueAttr = prop.GetCustomAttribute<DefaultValueAttribute>();
                if (defaultValueAttr != null)
                {
                    var defaultValueProperty = argumentInstanceType.GetProperty("DefaultValueFactory",
                        BindingFlags.Public | BindingFlags.Instance);
                    if (defaultValueProperty == null)
                        throw new InvalidOperationException(
                            $"Could not find property DefaultValueFactory on type {argumentInstanceType}");
                    defaultValueProperty.SetValue(argumentInstance, (ArgumentResult _) => defaultValueAttr.Value?.ToString());
                }
            }
            string[] GenerateAliases(
                PropertyInfo prop)
            {
                var actionParamAttr = prop.GetCustomAttribute<ActionParameterAttribute>();
                if (actionParamAttr == null)
                    return Array.Empty<string>();

                // create main alias
                var name = actionParamAttr.Name.Trim().Replace(' ', '-');
                if (name.StartsWith("--") == false)
                    name = $"--{name}";

                var argumentAliases = new List<string>() { name };

                // register short name alias
                if (string.IsNullOrEmpty(actionParamAttr.ShortNameAlias) == false)
                {
                    var shortName = actionParamAttr.ShortNameAlias.Trim();
                    if (!shortName.StartsWith("-"))
                    {
                        shortName = "-" + shortName;
                    }
                    argumentAliases.Add(shortName);
                }

                // add extra aliases
                foreach (var aliasAttr in prop.GetCustomAttributes<ActionParameterAliasAttribute>())
                {
                    if (!string.IsNullOrEmpty(aliasAttr.Alias))
                    {
                        var a = BuildAlias(aliasAttr);
                        if (!argumentAliases.Contains(a))
                        {
                            argumentAliases.Add(a);
                        }
                    }
                }

                return argumentAliases.ToArray();
            }

            foreach (var prop in optionsProps)
            {
                var actionParamAttr = prop.GetCustomAttribute<ActionParameterAttribute>();
                if (actionParamAttr != null)
                {
                    var genericArgumentType = typeof(Option<>).MakeGenericType(prop.PropertyType);
                    var argumentAliases = GenerateAliases(prop);
                    var argumentAliasesM = argumentAliases.ToList();
                    argumentAliasesM.RemoveAt(0);
                    var argumentInstance = Activator.CreateInstance(
                        genericArgumentType,
                        argumentAliases.Length == 1 ? new object[]
                        {
                            argumentAliases[0], Array.Empty<string>()
                        }
                        : new object[]
                        {
                            argumentAliases[0],
                            argumentAliasesM.ToArray()
                        });
                    if (argumentInstance == null)
                    {
                        throw new InvalidOperationException($"Failed to create instance of {genericArgumentType}");
                    }

                    genericArgumentType.GetProperty(nameof(Option.Required))?.SetValue(argumentInstance, actionParamAttr.IsRequired);

                    // set default value
                    SetDefaultValue(prop, genericArgumentType, argumentInstance);
                    
                    // Set AllowMultipleArgumentsPerToken
                    if (actionParamAttr.AllowMultipleArgumentsPerToken)
                    {
                        var p = genericArgumentType.GetProperty(nameof(Option.AllowMultipleArgumentsPerToken),  BindingFlags.Public | BindingFlags.Instance);
                        if (p == null)
                        {
                            throw new InvalidOperationException(
                                string.Format("Failed to get property {0} on {1}",
                                    nameof(Option.AllowMultipleArgumentsPerToken),
                                    genericArgumentType));
                        }
                        p.SetValue(argumentInstance, true);
                    }
                    
                    argumentPropertyMap.Add(prop.Name, (argumentInstance, actionParamAttr));
                }
            }

            var cmd = new Command(commandName, commandHelpText);
            foreach (var argValue in argumentPropertyMap.Values.Select(e => e.Item1))
            {
                var addArgumentMethod = typeof(Command).GetMethods()
                    .FirstOrDefault(e =>
                    {
                        var cond = e.Name == "Add" && e.IsPublic;
                        var p = e.GetParameters();
                        cond &= p.Length == 1 && p[0].ParameterType == typeof(Option);
                        return cond;
                    });
                if (addArgumentMethod != null)
                {
                    _ = addArgumentMethod.Invoke(cmd, new object[]
                    {
                        argValue
                    });
                }
            }

            Task SetActionCallback(ParseResult ctx)
            {
                var options = Activator.CreateInstance(optionsType);
                if (options == null)
                {
                    throw new ArgumentException($"Failed to create instance of {optionsType}", nameof(optionsType));
                }
                foreach (var kvPair in argumentPropertyMap)
                {
                    var key = kvPair.Key;
                    var argValue = kvPair.Value.Item1;

                    var prop = optionsType.GetProperty(key);
                    if (prop != null)
                    {
                        var getValueMethod = typeof(ParseResult)
                            .GetMethods()
                            .FirstOrDefault(v => v.Name == "GetValue"
                                                 && v.IsGenericMethod
                                                 && (v.GetParameters().FirstOrDefault()?.ParameterType.ToString().StartsWith("System.CommandLine.Option") ?? false)
                                                 && (v.GetParameters().FirstOrDefault()?.ParameterType.IsGenericType ?? false));
                        if (getValueMethod == null)
                        {
                            var msg = string.Format("Failed to get method {0}.{1} on {2}",
                                nameof(ctx),
                                nameof(ctx.GetValue),
                                FormatTypeName(typeof(ParseResult)));
                            throw new ApplicationException(msg);
                        }
                        var getValueMethodGeneric = getValueMethod.MakeGenericMethod(prop.PropertyType);
                        if (getValueMethodGeneric == null)
                        {
                            var msg = string.Format("Could not make generic method for {0}.{1} where type is {2} on {3}",
                                nameof(ctx),
                                nameof(ctx.GetValue),
                                FormatTypeName(prop.PropertyType),
                                FormatTypeName(typeof(ParseResult)));
                            throw new ApplicationException(msg);
                        }
                        var argumentValue = getValueMethodGeneric.Invoke(ctx, new object[] { argValue });
                        prop.SetValue(options, argumentValue);
                    }
                }
                return handler(options, ctx);
            }

            Func<ParseResult, Task> callback = SetActionCallback;
            cmd.SetAction(callback);
            return cmd;
        }

        /// <summary>
        /// Generate an instance of <see cref="Command"/>
        /// </summary>
        /// <param name="optionsType">
        /// Type of the options to deserialize.</param>
        /// <param name="commandName">
        /// Name of the command.</param>
        /// <param name="commandHelpText">
        /// Help text for the command</param>
        /// <param name="handler">
        /// Handler to be called when the generated command was ran.</param>
        /// <returns>Instance of <see cref="Command"/></returns>
        public static Command GenerateCommand(Type optionsType, string commandName, string commandHelpText, Func<object, Task> handler)
        {
            return GenerateCommand(
                optionsType,
                commandName,
                commandHelpText,
                (a, b) => handler(a));
        }

        /// <summary>
        /// Generate an instance of <see cref="Command"/> dynamically with the options type, and the type of the 
        /// class that should be created and called when the command has been invoked.
        /// </summary>
        /// <param name="optionsType">
        /// Type of the options. Properties will only be included if they have <see cref="ActionParameterAttribute"/> on it.
        /// </param>
        /// <param name="actionType">
        /// Type of the class that implements <see cref="IAction"/>.
        /// This type must also have <see cref="CommandActionAttribute"/> on it.
        /// </param>
        /// <returns>Generated instance of <see cref="Command"/></returns>
        /// 
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="actionType"/> doesn't implement <see cref="IAction"/>
        /// </exception>
        public static Command GenerateCommand(Type optionsType, Type actionType)
        {
            if (!typeof(IAction).IsAssignableFrom(actionType))
            {
                throw new ArgumentException($"Class must implement {nameof(IAction)}", nameof(actionType));
            }
            var cmdActionAttr = actionType.GetCustomAttribute<CommandActionAttribute>();
            if (cmdActionAttr == null)
            {
                var msg = string.Format("Attribute {0} does not exist on type {1}",
                    FormatTypeName(typeof(CommandActionAttribute)),
                    actionType.ToString());
                throw new ArgumentException(msg, nameof(actionType));
            }

            return GenerateCommand(
                optionsType,
                cmdActionAttr.ActionName,
                cmdActionAttr.DisplayName,
                async (opts, ctx) =>
                {
                    var actionInstance = (IAction)Activator.CreateInstance(actionType);
                    await actionInstance.RunAsync(opts);
                });
        }

        /// <summary><inheritdoc cref="GenerateCommand(Type, Type)" path="/summary"/></summary>
        /// <typeparam name="TOptions"><inheritdoc cref="GenerateCommand(Type, Type)" path="/param[@name='optionsType']"/></typeparam>
        /// <typeparam name="TAction"><inheritdoc cref="GenerateCommand(Type, Type)" path="/param[@name='actionType']"/></typeparam>
        /// <returns><inheritdoc cref="GenerateCommand(Type, Type)" path="/returns"/></returns>
        public static Command GenerateCommand<TOptions, TAction>()
            where TOptions : class
            where TAction : class, IAction
        {
            return GenerateCommand(typeof(TOptions), typeof(TAction));
        }

        private static string FormatTypeName(Type type)
        {
            return type.Namespace + '.' + type.Name;
        }
    }
}