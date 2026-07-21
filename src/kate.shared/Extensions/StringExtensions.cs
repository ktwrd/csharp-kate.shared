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
using System.ComponentModel;
using System.Text;

namespace kate.shared.Extensions
{
    /// <summary>
    /// Extensions for strings.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Read the lines from a string. Currently supports; LF, CR, and CRLF.
        /// </summary>
        /// <returns>List of strings with the parsed lines.</returns>
        public static IList<string> ParseLines(this string content)
        {
            var lines = new List<string>();
            var sb = new StringBuilder();

            var previousChar = (char)0;
            const char lf = (char)10;
            const char cr = (char)13;
            for (int i = 0; i < content.Length; i++)
            {
                var c = content[i];
                // not any form of line ending, which means
                // it's safe to append to working string.
                if (c != cr && c != lf)
                {
                    sb.Append(c);
                }

                switch (c)
                {
                    // current char is LF and previous isn't CR
                    // that way we know it's just LF line endings
                    // and not CRLF line endings
                    case lf when previousChar != cr:
                        lines.Add(sb.ToString());
                        sb.Clear();
                        break;
                    // always safe to add CR line endings
                    // since in all common line endings,
                    // CR is before LF or LF doesn't exist
                    // at all so it's just CR.
                    case cr:
                        lines.Add(sb.ToString());
                        sb.Clear();
                        break;
                }

                previousChar = c;
            }
            lines.Add(sb.ToString());
            return lines;
        }
    
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
        public static string ToDescriptionString<T>(this T value, string fallback) where T : struct
        {
            if (value.ToString() == null)
                return fallback;
            var attributes = (DescriptionAttribute[])value
               .GetType()
               .GetField(value.ToString())
               .GetCustomAttributes(typeof(DescriptionAttribute), false);
            return attributes?.Length > 0 ? attributes[0].Description : fallback;
        }

        /// <summary>
        /// Get the value of the <see cref="DescriptionAttribute"/> on an object, if it is on it.
        /// </summary>
        /// <returns>Empty string when no <see cref="DescriptionAttribute"/> found</returns>
        public static string ToDescriptionString<T>(this T value) where T : struct
        {
            return value.ToDescriptionString(string.Empty);
        }

        /// <summary>
        /// Parse a string (that is a byte array in hexadecimal) to a byte
        /// array.
        /// </summary>
        /// <param name="value">String that is hexadecimal.</param>
        /// <returns></returns>
        /// <exception cref="Exception">Thrown when the length of
        /// <paramref name="value"/> is an odd number.</exception>
        [Obsolete("Use extension method ParseAsHexadecimal(this string)")]
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
        /// Parse a string (that is a byte array in hexadecimal, like <c>0x00</c> or <c>00</c>)
        /// to a byte array.
        /// </summary>
        /// 
        /// <returns>
        /// Result will be an empty array when <paramref name="value"/> is <see langword="null"/> or empty.
        /// Otherwise, the parsed array will be returned (e.g, value <c>0xaaff</c> will return an array like <c>[177, 255]</c>).
        /// </returns>
        /// 
        /// <exception cref="ArgumentException">
        /// Thrown when the length of the real string is not an even number.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when something went wrong while parsing two characters to a byte. The positions may be incorrect since 
        /// </exception>
        /// 
        /// <remarks>
        /// <para>
        /// When reading the <paramref name="value"/>, it is converted into a "real" string.
        /// This "real" string is trimmed, then anything before the first occupance of <c>x</c> (including that char)
        /// is ignored when parsing it to a byte array.
        /// </para>
        /// 
        /// <para>As an example, the real string of <c>0x003</c> would actually be <c>003</c></para>
        /// 
        /// <para>When the <paramref name="value"/> is null, empty, or equal to <c>System.Byte[]</c>, then an empty array will be returned.</para>
        /// </remarks>
        public static byte[] ParseAsHexadecimal(this string value)
        {
            if (value == null || string.IsNullOrEmpty(value.Trim()) || value == "System.Byte[]")
            {
                return Array.Empty<byte>();
            }

            value = value.TrimWithOffsets(out var removedFromStart, out var removedFromEnd);

            var pad = value.IndexOf('x');
            if (pad == -1) pad = value.IndexOf('X');
            var start = 0;
            if (pad != -1)
            {
                start = pad + 1;
            }

            var arrayLength = value.Length - start;
            byte[] arr = new byte[arrayLength >> 1];

            for (int i = start; i < value.Length >> 1; ++i)
            {
                char? ca = null;
                char? cb = null;
                try
                {
                    ca = value[i << 1];
                    cb = value[(i << 1) + 1];
                    arr[i - start] = (byte)((ca.Value.GetHexVal() << 4) + cb.Value.GetHexVal());
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("Failed to parse characters " + ca + "," + cb + "  at positions " + (i << 1) + removedFromStart + "," + ((i << 1) + 1) + removedFromStart, ex);
                }
            }

            return arr;
        }

        public static string TrimWithOffsets(this string value, out int removedFromStart, out int removedFromEnd)
        {
            var previousLength = value.Length;
            var result = value.TrimEnd();
            removedFromEnd = previousLength - result.Length;

            previousLength = result.Length;
            result = result.TrimStart();
            removedFromStart = previousLength - result.Length;
            return result;
        }
        
        /// <summary>
        /// Trim <paramref name="value"/> when it is not <see langword="null"/>.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string TrimOrNull(this string value)
        {
            if (value == null) return null;
            return value.Trim();
        }

        /// <summary>
        /// Parse the character (that is [0-9a-fA-F], hex) as an <see cref="int"/>.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when value isn't 0-9 or a-f (case-insensitive).</exception>
        public static int GetHexVal(this char hex)
        {
            int val = (int)hex;
            //For uppercase A-F letters:
            //return val - (val < 58 ? 48 : 55);
            //For lowercase a-f letters:
            //return val - (val < 58 ? 48 : 87);
            //Or the two combined, but a bit slower:
            var result = val - (val < 58 ? 48 : (val < 97 ? 55 : 87));
            if (result > 16)
                throw new ArgumentException("Value is not valid hexadecimal since it is greater than 15 (got char: \"" + hex + "\" got number: " + result + ", min: 0, max: 15)");
            return result;
        }
    }
}
