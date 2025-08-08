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

namespace kate.shared.Helpers
{
    public static class PointerHelper
    {
        public static unsafe T[] CreateArray<T>(void* t, int length, int byteSize) where T : struct
        {
            T[] result = new T[length];
            for (int i = 0; i < length; i++)
            {
                IntPtr p = new IntPtr((byte*)t + (i * byteSize));
                result[i] = (T)System.Runtime.InteropServices.Marshal.PtrToStructure(p, typeof(T));
            }

            return result;
        }

        public static unsafe IntPtr ToPointer(int[] target)
        {
            int[] rawdata = new int[target.Length];

            Array.Copy(target, rawdata, Math.Min(target.Length, rawdata.Length));

            IntPtr? dataPointer;
            fixed (int* bptr = rawdata)
            {
                dataPointer = new IntPtr(bptr);
            }
            return dataPointer ?? IntPtr.Zero;
        }
        public static unsafe IntPtr ToPointer(float[] target)
        {
            float[] rawdata = new float[target.Length];

            Array.Copy(target, rawdata, Math.Min(target.Length, rawdata.Length));

            IntPtr? dataPointer;
            fixed (float* bptr = rawdata)
            {
                dataPointer = new IntPtr(bptr);
            }

            return dataPointer ?? IntPtr.Zero;
        }
        public static unsafe IntPtr ToPointer(double[] target)
        {
            double[] rawdata = new double[target.Length];

            Array.Copy(target, rawdata, Math.Min(target.Length, rawdata.Length));

            IntPtr? dataPointer;
            fixed (double* bptr = rawdata)
            {
                dataPointer = new IntPtr(bptr);
            }

            return dataPointer ?? IntPtr.Zero;
        }
        public static unsafe IntPtr ToPointer(short[] target)
        {
            short[] rawdata = new short[target.Length];

            Array.Copy(target, rawdata, Math.Min(target.Length, rawdata.Length));

            IntPtr? dataPointer;
            fixed (short* bptr = rawdata)
            {
                dataPointer = new IntPtr(bptr);
            }

            return dataPointer ?? IntPtr.Zero;
        }
        public static unsafe IntPtr ToPointer(char[] target)
        {
            char[] rawdata = new char[target.Length];

            Array.Copy(target, rawdata, Math.Min(target.Length, rawdata.Length));

            IntPtr? dataPointer;
            fixed (char* bptr = rawdata)
            {
                dataPointer = new IntPtr(bptr);
            }

            return dataPointer ?? IntPtr.Zero;
        }
    }
}
