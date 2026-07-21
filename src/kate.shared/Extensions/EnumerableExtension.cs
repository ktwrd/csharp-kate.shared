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
using System.Linq;
using System.Text;

namespace kate.shared.Extensions
{
    /// <summary>
    /// Extensions for <see cref="IEnumerable"/>
    /// </summary>
    public static class EnumerableExtension
    {
        /// <summary>
        /// Get a random item from the Enumerable provided.
        /// </summary>
        /// <typeparam name="TValue">Type of the value that is in this Enumerable.</typeparam>
        /// <param name="items">Enumerable</param>
        /// <param name="random">Random Number Generator. Will create one when none provided.</param>
        /// <returns>Random value.</returns>
        public static TValue GetRandom<TValue>(this IEnumerable<TValue> items, Random random = null)
        {
            if (random == null)
            {
                random = new Random();
            }
            var index = random.Next(0, items.Count());
            return items.ElementAt(index);
        }
    }
}
