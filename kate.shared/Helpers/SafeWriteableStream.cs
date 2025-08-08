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
using System.IO;

namespace kate.shared.Helpers
{
    public class SafeWriteStream : FileStream
    {
        static object SafeLock = new object(); //ensure we are only ever writing one stream to disk at a time, application wide.

        private bool aborted;

        string finalFilename;
        string temporaryFilename;
        public SafeWriteStream(string filename) : base(filename + "." + DateTime.Now.Ticks, FileMode.Create)
        {
            temporaryFilename = this.Name;
            finalFilename = filename;
        }

        ~SafeWriteStream()
        {
            if (!isDisposed) Dispose(false);
            return;
        }

        public void Abort()
        {
            aborted = true;
        }

        bool isDisposed;
        protected override void Dispose(bool disposing)
        {
            if (isDisposed) return;
            isDisposed = true;

            base.Dispose(disposing);

            lock (SafeLock)
            {
                if (!File.Exists(temporaryFilename)) return;

                if (aborted)
                {
                    GeneralHelper.FileDelete(temporaryFilename);
                    return;
                }

                try
                {
                    GeneralHelper.FileMove(temporaryFilename, finalFilename);
                }
                catch
                {
                }
            }
        }
    }
}
