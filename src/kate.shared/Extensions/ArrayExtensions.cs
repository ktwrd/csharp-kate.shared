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
                throw new ArgumentOutOfRangeException(nameof(newIndex), $"Value does not exist in this array");
            if (oldIndex >= array.Length)
                throw new ArgumentOutOfRangeException(nameof(oldIndex), $"Value does not exist in this array");

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
