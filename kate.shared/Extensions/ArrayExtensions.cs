using System;
using System.Collections.Generic;
using System.Text;

namespace kate.shared.Extensions
{
    /// <summary>
    /// Extensions for arrays.
    /// </summary>
    public static class ArrayExtensions
    {
        /// <summary>
        /// Swap the contents at the index of <paramref name="oldIndex"/> with the contents at <paramref name="newIndex"/>
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static void ShiftElement<T>(this T[] array, int oldIndex, int newIndex)
        {
            if (oldIndex == newIndex)
                return;
            if (newIndex >= array.Length)
                throw new ArgumentOutOfRangeException($"{nameof(newIndex)} does not exist in this array");
            if (oldIndex >= array.Length)
                throw new ArgumentOutOfRangeException($"{nameof(oldIndex)} does not exist in this array");

            var tmpArray = new T[array.Length];
            array.CopyTo(tmpArray, 0);
            for (int i = 0; i < array.Length; i++)
            {
                if (i == oldIndex)
                {
                    array[newIndex] = tmpArray[oldIndex];
                }
                else if (i == newIndex)
                {
                    array[oldIndex] = tmpArray[newIndex];
                }
                else
                {
                    array[i] = tmpArray[i];
                }
            }
        }
    }
}
