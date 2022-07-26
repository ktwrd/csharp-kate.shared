using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace kate.shared.Helpers
{
    public class DatabaseHelper
    {
        public delegate void ReadHandler(SerializationReader reader);
        public delegate void WriteHandler(SerializationWriter writer);

        public static bool Read(string filename, ReadHandler handler, ExceptionDelegate onFail = null)
        {
            if (!File.Exists(filename)) return false;

            try
            {
                using (Stream stream = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (SerializationReader reader = new SerializationReader(stream))
                {
                    handler(reader);
                    return true;
                }
            }
            catch (Exception e)
            {
                if (onFail != null)
                    onFail.Invoke(e);

                try
                {
                    GeneralHelper.CreateBackup(filename);
                }
                catch { }

                return false;
            }
        }

        public static bool Write(string filename, WriteHandler handler)
        {
            try
            {
                using (SafeWriteStream stream = new SafeWriteStream(filename))
                using (SerializationWriter writer = new SerializationWriter(stream))
                {
                    try
                    {
                        handler(writer);
                        return true;
                    }
                    catch (Exception e)
                    {
                        stream.Abort();
                        throw;
                    }
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
                return false;
            }
        }

    }
}
