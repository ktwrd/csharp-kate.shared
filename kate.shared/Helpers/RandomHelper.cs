using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace kate.shared.Helpers
{
    /// <summary>
    /// Helper class for stuff relating to <see cref="Random"/>.
    /// </summary>
    public static class RandomHelper
    {
        /// <summary>
        /// Get random item based off percentage chance.
        /// </summary>
        /// <example>
        /// // 75% chance of 'A'
        /// // 5% chance of 'B'
        /// // 5% chance of 'C'
        /// // 15% chance of 'Z'
        /// var result = RandomHelper.GetChanceItem{char}(new List{char, double}()
        /// {
        ///     {'A', 0.75d},
        ///     {'B', 0.05d},
        ///     {'C', 0.05d}
        /// }, 'Z');
        /// </example>
        /// <typeparam name="T">Type of the result value.</typeparam>
        public static T GetChanceItem<T>(List<RandomChanceItem<T>> items, T defaultValue)
        {
            var random = new Random();
            var sorted = items.OrderByDescending(v => v.Chance).ToList();
            for (int i = 0; i < sorted.Count; i++)
            {
                var value = random.NextDouble();
                var item = sorted[i];
                var allPreviousCombined = sorted.Where((v, idx) => idx > i).Sum(v => v.Chance);
                if (value < allPreviousCombined)
                    return item.Value;
            }
            return defaultValue;
        }
        /// <summary>
        /// Same as <see cref="GetChanceItem{T}(List{RandomChanceItem{T}}, T)"/> but it will cast (T, double) to <see cref="RandomChanceItem{T}"/>
        /// </summary>
        public static T GetChanceItem<T>(List<(T, double)> items, T defaultValue)
        {
            return GetChanceItem(
                items.Select(v => new RandomChanceItem<T>(v.Item1, v.Item2)).ToList(),
                defaultValue);
        }
    }
    /// <summary>
    /// Item for randomly deciding stuff with <see cref="RandomHelper.GetChanceItem{T}(List{RandomChanceItem{T}}, T)"/>
    /// </summary>
    /// <typeparam name="T">Type of <see cref="RandomChanceItem{T}.Value"/></typeparam>
    public class RandomChanceItem<T>
    {
        /// <summary>
        /// Value to use
        /// </summary>
        public T Value;
        /// <summary>
        /// Percentage chance for this item being picked. (0.0f to 1.0f)
        /// </summary>
        public double Chance;
        /// <summary>
        /// Create an instance of this.
        /// </summary>
        /// <param name="value">Sets <see cref="Value"/></param>
        /// <param name="chance">Sets <see cref="Chance"/></param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="chance"/> is less than 0.0d or greater than 1.0d</exception>
        public RandomChanceItem(T value, double chance)
        {
            if (chance < 0.0d || chance > 1.0d)
                throw new ArgumentOutOfRangeException("chance must be between 0f and 1f");
            Value = value;
            Chance = chance;
        }
    }
}
