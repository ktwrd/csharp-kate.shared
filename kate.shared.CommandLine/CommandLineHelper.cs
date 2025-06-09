using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using ParseResult = System.CommandLine.Parsing.ParseResult;

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
        public static Command GenerateCommand<TOptions>(string commandName, string commandHelpText, Func<TOptions, InvocationContext, Task> handler)
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
            Func<object, InvocationContext, Task> handler)
        {
            var argumentPropertyMap = new Dictionary<string, (object, ActionParameterAttribute)>();
            var optionsProps = optionsType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (var prop in optionsProps)
            {
                var actionParamAttr = prop.GetCustomAttribute<ActionParameterAttribute>();
                if (actionParamAttr != null)
                {
                    var genericArgumentType = typeof(Option<>).MakeGenericType(prop.PropertyType);
                    var name = actionParamAttr.Name;
                    if (name.StartsWith("--") == false)
                        name = $"--{name}";
                    var argumentInstance = Activator.CreateInstance(genericArgumentType, new object[] { name, actionParamAttr.HelpText });
                    if (string.IsNullOrEmpty(actionParamAttr.ShortNameAlias) == false)
                    {
                        var shortName = actionParamAttr.ShortNameAlias;
                        if (shortName.StartsWith("-") == false)
                        {
                            shortName = "-" + shortName;
                        }
                        argumentInstance = Activator.CreateInstance(genericArgumentType, new object[] { new string[] { name, shortName }, actionParamAttr.HelpText });
                    }
                    typeof(Option<>).GetProperty(nameof(Option.IsRequired))?.SetValue(argumentInstance, actionParamAttr.IsRequired);
                    argumentPropertyMap.Add(prop.Name, (argumentInstance, actionParamAttr));
                }
            }

            var cmd = new Command(commandName, commandHelpText);
            foreach (var argValue in argumentPropertyMap.Values.Select(e => e.Item1))
            {
                var addArgumentMethod = typeof(Command).GetMethod("AddOption");
                if (addArgumentMethod != null)
                {
                    _ = addArgumentMethod.Invoke(cmd, new object[] { argValue });
                }
            }
            cmd.SetHandler(async (ctx) =>
            {
                var options = Activator.CreateInstance(optionsType);
                foreach (var kvPair in argumentPropertyMap)
                {
                    var key = kvPair.Key;
                    var argValue = kvPair.Value.Item1;

                    var prop = optionsType.GetProperty(key);
                    if (prop != null)
                    {
                        var getValueMethod = typeof(ParseResult)
                            .GetMethods()
                            .Where(v => v.Name =="GetValueForOption" && v.IsGenericMethod == true
                                                 && (v.GetParameters().FirstOrDefault()?.ParameterType.ToString().StartsWith("System.CommandLine.Option") ?? false)
                                                 && (v.GetParameters().FirstOrDefault()?.ParameterType.IsGenericType ?? false))
                            .FirstOrDefault();
                        if (getValueMethod == null)
                        {
                            var msg = string.Format("Failed to get method {0}.{1}.{2} on {3}",
                                nameof(ctx),
                                nameof(ctx.ParseResult),
                                nameof(ctx.ParseResult.GetValueForOption),
                                FormatTypeName(typeof(ParseResult)));
                            throw new ApplicationException(msg);
                        }
                        var getValueMethodGeneric = getValueMethod.MakeGenericMethod(prop.PropertyType);
                        if (getValueMethodGeneric == null)
                        {
                            var msg = string.Format("Could not make generic method for {0}.{1}.{2} where type is {3} on {4}",
                                nameof(ctx),
                                nameof(ctx.ParseResult),
                                nameof(ctx.ParseResult.GetValueForOption),
                                FormatTypeName(prop.PropertyType),
                                FormatTypeName(typeof(ParseResult)));
                            throw new ApplicationException(msg);
                        }
                        var argumentValue = getValueMethodGeneric.Invoke(ctx.ParseResult, new object[] { argValue });
                        prop.SetValue(options, argumentValue);
                    }
                }
                await handler(options, ctx);
            });
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
            if (typeof(IAction).IsAssignableFrom(actionType) == false)
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
