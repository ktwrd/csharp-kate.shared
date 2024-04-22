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
