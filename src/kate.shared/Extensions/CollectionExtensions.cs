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
using System.Linq;
using System.Text;

namespace kate.shared.Extensions
{
    // <summary>
    /// Extension methods for <see cref="System.Collections.Generic"/>
    /// </summary>
    public static class CollectionsExtensions
    {
        /// <summary>
        /// <para>Set the Value of a Key in a Dictionary.</para>
        /// 
        /// <para>This also Locks the dictionary to prevent cross-thread operations.</para>
        /// </summary>
        /// <typeparam name="TKey">Type of the Key that is used in the <paramref name="dict"/> provided.</typeparam>
        /// <typeparam name="TValue">Type of the Value that is used in the <paramref name="dict"/> provided.</typeparam>
        /// <param name="dict">Dictionary instance</param>
        /// <param name="key">Key to set the Value on</param>
        /// <param name="value">Value to set.</param>
        public static void Set<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue value)
        {
            lock (dict)
            {
                if (dict.ContainsKey(key) == false)
                {
                    dict.Add(key, value);
                }
                dict[key] = value;
            }
        }
        /// <summary>
        /// Wrapper for the <see cref="ICollection{T}.Add(T)"/> method to lock before adding.
        /// </summary>
        public static void AddLock<TValue>(this ICollection<TValue> list, TValue value)
        {
            lock (list)
            {
                list.Add(value);
            }
        }
        /// <summary>
        /// <para>Initialize the value of <typeparamref name="TValue"/> to a new instance of it (with an empty constructor).</para>
        /// 
        /// <para>It will only set the value when it's not set.</para>
        /// </summary>
        /// <typeparam name="TKey">Type of the Key that is used in the <paramref name="dict"/> provided.</typeparam>
        /// <typeparam name="TValue">Type of the Value that is used in the <paramref name="dict"/> provided.</typeparam>
        /// <param name="dict">Dictionary instance</param>
        /// <param name="key">Key to set the Value on</param>
        public static void InitValue<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key)
            where TValue : new()
        {
            lock (dict)
            {
                if (dict.ContainsKey(key) == false)
                {
                    dict.Set(key, new TValue());
                }
            }
        }

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
