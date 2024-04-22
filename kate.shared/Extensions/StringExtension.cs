using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace kate.shared.Extensions
{
    /// <summary>
    /// Extensions for strings.
    /// </summary>
    public static class StringExtension
    {
        /// <summary>
        /// Split a string by a separator.
        /// </summary>
        /// <param name="value">String</param>
        /// <param name="separator">Separator.</param>
        /// <returns>Splitted string.</returns>
        public static string[] Split(this string value, string separator)
        {
            return value.Split(separator.ToCharArray());
        }
        /// <summary>
        /// Get the value of the <see cref="DescriptionAttribute"/> on an object, if it is on it.
        /// </summary>
        /// <returns>Empty string when no <see cref="DescriptionAttribute"/> found</returns>
        public static string ToDescriptionString<T>(this T value) where T : struct
        {
            if (value.ToString() == null)
                return "";
            var attributes = (DescriptionAttribute[])value
               .GetType()
               .GetField(value.ToString())
               .GetCustomAttributes(typeof(DescriptionAttribute), false);
            return attributes?.Length > 0 ? attributes[0].Description : string.Empty;
        }
    }
}
