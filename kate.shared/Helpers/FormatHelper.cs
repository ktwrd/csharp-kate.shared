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

namespace kate.shared.Helpers
{
    /// <summary>
    /// Helper class for formatting things
    /// </summary>
    public static class FormatHelper
    {
        public const string SqlServerDateTimeFormat = "yyyy-MM-dd HH:mm:ss.fff";

        /// <summary>
        /// Format a TimeSpan to either one of the following;
        /// <list type="bullet">
        /// <item><c>0d 0h 0m 0s</c></item>
        /// <item><c>0h 0m 0s</c></item>
        /// <item><c>0m 0s</c></item>
        /// <item><c>0s</c></item>
        /// <item><c>0d 0h 0m 0.000s</c></item>
        /// <item><c>0h 0m 0.000s</c></item>
        /// <item><c>0m 0.000s</c></item>
        /// <item><c>0.000s</c></item>
        /// <item><c>0ms</c> (when <see cref="TimeSpan.TotalMilliseconds"/> is less than <c>2000</c>)</item>
        /// </list>
        /// </summary>
        public static string Duration(TimeSpan span)
        {
            if (span.TotalMilliseconds < 2000)
            {
                return span.TotalMilliseconds.ToString() + "ms";
            }
            var result = new List<string>();

            if (span.Milliseconds > 10)
            {
                result.Add(string.Format("{0}.{1}",
                    span.Seconds,
                    span.Milliseconds.ToString().PadLeft(3, '0')));
            }
            else
            {
                result.Add(span.Seconds.ToString() + 's');
            }

            if (span.Minutes > 0)
            {
                result.Insert(0,
                    string.Format("{0}m", span.Minutes));
            }
            if (span.Hours > 0)
            {
                result.Insert(0,
                    string.Format("{0}h", span.Hours));
            }
            if (span.Days > 0)
            {
                result.Insert(0,
                    string.Format("{0}d", span.Days));
            }
            return string.Join(" ", result);
        }

        /// <summary>
        /// Format the duration between <paramref name="start"/> and the current date (using <see cref="DateTimeOffset.UtcNow"/>)
        /// with <see cref="Duration(TimeSpan)"/>
        /// </summary>
        public static string DurationToUtcNow(DateTimeOffset start)
        {
            var end = DateTimeOffset.UtcNow;
            var diff = end - start;

            return Duration(diff);
        }
        
        /// <summary>
        /// Format the duration between <paramref name="start"/> and the current date (using <see cref="DateTime.UtcNow"/>)
        /// with <see cref="Duration(TimeSpan)"/>
        /// </summary>
        public static string DurationToUtcNow(DateTime start)
        {
            var end = DateTime.UtcNow;
            var diff = end - start;

            return Duration(diff);
        }
        
        /// <summary>
        /// Format the duration between <paramref name="start"/> and the current date (using <see cref="DateTimeOffset.Now"/>)
        /// with <see cref="Duration(TimeSpan)"/>
        /// </summary>
        public static string DurationToNow(DateTimeOffset start)
        {
            var end = DateTimeOffset.Now;
            var diff = end - start;

            return Duration(diff);
        }

        /// <summary>
        /// Format the duration between <paramref name="start"/> and the current date (using <see cref="DateTime.Now"/>)
        /// with <see cref="Duration(TimeSpan)"/>
        /// </summary>
        public static string DurationToNow(DateTime start)
        {
            var end = DateTime.Now;
            var diff = end - start;

            return Duration(diff);
        }
        public static string DurationMilliseconds(long value)
        {
            return Duration(TimeSpan.FromMilliseconds(value));
        }
#if NET8_0_OR_GREATER
        public static string DurationMicroseconds(long value)
        {
            return Duration(TimeSpan.FromMicroseconds(value));
        }
#endif
    }
}
