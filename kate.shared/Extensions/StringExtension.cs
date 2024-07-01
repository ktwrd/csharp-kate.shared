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
        /// <summary>
        /// Parse a string (that is a byte array in hexadecimal) to a byte
        /// array.
        /// </summary>
        /// <param name="value">String that is hexadecimal.</param>
        /// <returns></returns>
        /// <exception cref="Exception">Thrown when the length of
        /// <paramref name="value"/> is an odd number.</exception>
        public static byte[] ParseToByteArray(this string value)
        {
            if (value == "System.Byte[]")
            {
                return Array.Empty<byte>();
            }
            string hex = $"{value}";
            if (hex.Length % 2 == 1)
                throw new Exception("The binary key cannot have an odd number of digits");

            var pad = hex.IndexOf('x');
            if (pad != -1)
            {
                hex = hex.Substring(pad + 1);
            }

            byte[] arr = new byte[hex.Length >> 1];

            for (int i = 0; i < hex.Length >> 1; ++i)
            {
                arr[i] = (byte)((hex[i << 1].GetHexVal() << 4) + hex[(i << 1) + 1].GetHexVal());
            }

            return arr;
        }
        /// <summary>
        /// Parse the character (that is [0-9a-fA-F], hex) as an <see cref="int"/>.
        /// </summary>
        public static int GetHexVal(this char hex)
        {
            int val = (int)hex;
            //For uppercase A-F letters:
            //return val - (val < 58 ? 48 : 55);
            //For lowercase a-f letters:
            //return val - (val < 58 ? 48 : 87);
            //Or the two combined, but a bit slower:
            return val - (val < 58 ? 48 : (val < 97 ? 55 : 87));
        }
    }
}
