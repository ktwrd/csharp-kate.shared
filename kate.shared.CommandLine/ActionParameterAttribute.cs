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
            IsRequired = required;
            HelpText = string.IsNullOrEmpty(helpText) ? "" : helpText;
        }

        public ActionParameterAttribute(Nullable<char> shortName, string name, string helpText)
            : this(shortName, name, true, helpText)
        { }

        public ActionParameterAttribute(Nullable<char> shortName, string name, bool required, string helpText)
            : base()
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name), "Cannot be null or empty");
            }
            Name = name;
            ShortNameAlias = shortName == null || !shortName.HasValue
                ? null
                : shortName.ToString();
            IsRequired = required;
            HelpText = string.IsNullOrEmpty(helpText) ? "" : helpText;
        }
    }
}
