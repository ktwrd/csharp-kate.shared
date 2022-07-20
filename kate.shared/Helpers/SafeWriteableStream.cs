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
            if (!isDisposed) Dispose();
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
