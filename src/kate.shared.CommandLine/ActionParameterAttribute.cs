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
using System.Text;

namespace kate.shared.CommandLine
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class ActionParameterAttribute : Attribute
    {
        /// <summary>
        /// Short-hand argument name, like <c>h</c> for <c>help</c>
        /// </summary>
        public string ShortNameAlias { get; private set; }

        /// <summary>
        /// Parameter name, must not start with <c>--</c>
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Help text to display for this parameter.
        /// </summary>
        public string HelpText { get; private set; }

        /// <summary>
        /// Is this parameter required? (Default: <see langword="true"/>)
        /// </summary>
        public bool IsRequired { get; set; } = true;
        
        /// <inheritdoc cref="System.CommandLine.Option.AllowMultipleArgumentsPerToken"/>
        public bool AllowMultipleArgumentsPerToken { get; set; }

        public ActionParameterAttribute(string name, string helpText)
            : this(name, true, helpText)
        { }

        public ActionParameterAttribute(string name, bool required, string helpText)
            : base()
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name), "Cannot be null or empty");
            }

            Name = name;
            ShortNameAlias = "";
            IsRequired = required;
            HelpText = string.IsNullOrEmpty(helpText) ? "" : helpText;
            AllowMultipleArgumentsPerToken = false;
        }

        public ActionParameterAttribute(char shortName, string name, string helpText)
            : this(shortName, name, true, helpText)
        { }

        public ActionParameterAttribute(char shortName, string name, bool required = false, string helpText = "")
            : base()
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name), "Cannot be null or empty");
            }
            Name = name;
            ShortNameAlias = shortName.ToString();
            if (string.IsNullOrEmpty(ShortNameAlias))
                ShortNameAlias = "";
            IsRequired = required;
            HelpText = string.IsNullOrEmpty(helpText) ? "" : helpText;
        }
    }
}