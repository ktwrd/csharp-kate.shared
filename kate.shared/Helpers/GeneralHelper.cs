using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Numerics;

namespace kate.shared.Helpers
{
    public static class GeneralHelper
    {
        public static T[] FixedArraySize<T>(T[] array, int length)
        {
            int maxIndex = Math.Min(length, array.Length);
            T[] result = new T[length];
            for (int i = 0; i < maxIndex; i++)
            {
                result[i] = array[i];
            }
            return result;
        }
        public static class Cast
        {
            public static double[] ToDoubleArray(short[] array)
            {
                var result = new double[array.Length];
                for (int i = 0; i < result.Length; i++)
                {
                    result[i] = float.Parse(array[i].ToString());
                }
                return result;
            }
            public static double[] ToDoubleArray(int[] array)
            {
                var result = new double[array.Length];
                for (int i = 0; i < result.Length; i++)
                {
                    result[i] = float.Parse(array[i].ToString());
                }
                return result;
            }
            public static float[] ToFloatArray(short[] array)
            {
                var result = new float[array.Length];
                for (int i = 0; i < result.Length; i++)
                {
                    result[i] = float.Parse(array[i].ToString());
                }
                return result;
            }
            public static float[] ToFloatArray(int[] array)
            {
                var result = new float[array.Length];
                for (int i = 0; i < result.Length; i++)
                {
                    result[i] = float.Parse(array[i].ToString());
                }
                return result;
            }
        }
        public static List<T> GetEnumList<T>()
        {
            T[] array = (T[])Enum.GetValues(typeof(T));
            List<T> list = new List<T>(array);
            return list;
        }
        public static void WriteStringToMemoryStream(MemoryStream stream, string content, int position=0)
        {
            var bytes = Encoding.UTF8.GetBytes(content);
            stream.Write(bytes, position, bytes.Length);
        }
        public static long GetNanoseconds()
        {
            double timestamp = Stopwatch.GetTimestamp();
            double nanoseconds = 1_000_000_000.0 * timestamp / Stopwatch.Frequency;

            return (long)nanoseconds;
        }
        public static string ToBase62(ulong number)
        {
            var alphabet = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
            var n = number;
            ulong basis = 62;
            var ret = "";
            while (n > 0)
            {
                ulong temp = n % basis;
                ret = alphabet[(int)temp] + ret;
                n /= basis;

            }
            return ret;
        }
        public static string Base62Encode(byte[] data)
        {
            var characters = "";
            foreach (var b in data)
            {
                characters += ToBase62(b);
            }
            return characters;
        }
        public static string GenerateToken(int length)
        {
            const string valid = "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            StringBuilder res = new StringBuilder();
            Random rnd = new Random();
            while (0 < length--)
            {
                res.Append(valid[rnd.Next(valid.Length)]);
            }
            return res.ToString();
        }
        public static string GenerateUID()
        {
            int length = 20;
            const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            StringBuilder res = new StringBuilder();
            Random rnd = new Random();
            while (0 < length--)
            {
                res.Append(valid[rnd.Next(valid.Length)]);
            }
            return res.ToString();
        }
        public const int MAX_PATH_LENGTH = 248;

        public const string CLEANUP_DIRECTORY = @"_cleanup";

        public static string FormatEnum(string s)
        {
            string o = string.Empty;
            for (int i = 0; i < s.Length; i++)
            {
                if (i > 0 && Char.IsUpper(s[i]))
                    o += @" ";
                o += s[i];
            }

            return o;
        }

        public static string WindowsFilenameStrip(string entry)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
                entry = entry.Replace(c.ToString(), string.Empty);
            return entry;
        }

        public static string WindowsPathStrip(string entry)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
                entry = entry.Replace(c.ToString(), string.Empty);
            entry = entry.Replace(".", string.Empty);
            return entry;
        }

        public static string UrlEncode(string str)
        {
            if (str == null)
            {
                return null;
            }
            return UrlEncode(str, Encoding.UTF8, false);
        }

        public static string UrlEncodeParam(string str)
        {
            if (str == null)
            {
                return null;
            }
            return UrlEncode(str, Encoding.UTF8, true);
        }

        public static string UrlEncode(string str, Encoding e, bool paramEncode)
        {
            if (str == null)
            {
                return null;
            }
            return Encoding.ASCII.GetString(UrlEncodeToBytes(str, e, paramEncode));
        }

        public static byte[] UrlEncodeToBytes(string str, Encoding e, bool paramEncode)
        {
            if (str == null)
            {
                return null;
            }
            byte[] bytes = e.GetBytes(str);
            return UrlEncodeBytesToBytespublic(bytes, 0, bytes.Length, false, paramEncode);
        }

        private static byte[] UrlEncodeBytesToBytespublic(byte[] bytes, int offset, int count, bool alwaysCreateReturnValue, bool paramEncode)
        {
            int num = 0;
            int num2 = 0;
            for (int i = 0; i < count; i++)
            {
                char ch = (char)bytes[offset + i];
                if (paramEncode && ch == ' ')
                {
                    num++;
                }
                else if (!IsSafe(ch))
                {
                    num2++;
                }
            }
            if ((!alwaysCreateReturnValue && (num == 0)) && (num2 == 0))
            {
                return bytes;
            }
            byte[] buffer = new byte[count + (num2 * 2)];
            int num4 = 0;
            for (int j = 0; j < count; j++)
            {
                byte num6 = bytes[offset + j];
                char ch2 = (char)num6;
                if (IsSafe(ch2))
                {
                    buffer[num4++] = num6;
                }
                else if (paramEncode && ch2 == ' ')
                {
                    buffer[num4++] = 0x2b;
                }
                else
                {
                    buffer[num4++] = 0x25;
                    buffer[num4++] = (byte)IntToHex((num6 >> 4) & 15);
                    buffer[num4++] = (byte)IntToHex(num6 & 15);
                }
            }
            return buffer;
        }

        public static bool IsSafe(char ch)
        {
            if ((((ch >= 'a') && (ch <= 'z')) || ((ch >= 'A') && (ch <= 'Z'))) || ((ch >= '0') && (ch <= '9')))
            {
                return true;
            }
            switch (ch)
            {
                case '\'':
                case '(':
                case ')':
                case '*':
                case '-':
                case '.':
                case '_':
                case '!':
                    return true;
            }
            return false;
        }

        public static char IntToHex(int n)
        {
            if (n <= 9)
            {
                return (char)(n + 0x30);
            }
            return (char)((n - 10) + 0x61);
        }

        public static void RemoveReadOnlyRecursive(string s)
        {
            foreach (string f in Directory.GetFiles(s))
            {
                FileInfo myFile = new FileInfo(f);
                if ((myFile.Attributes & FileAttributes.ReadOnly) > 0)
                    myFile.Attributes &= ~FileAttributes.ReadOnly;
            }

            foreach (string d in Directory.GetDirectories(s))
                RemoveReadOnlyRecursive(d);
        }

        public static bool FileMove(string src, string dest, bool overwrite = true)
        {
            src = PathSanitise(src);
            dest = PathSanitise(dest);
            if (src == dest)
                return true; //no move necessary

            try
            {
                if (overwrite)
                    FileDelete(dest);
                File.Move(src, dest);
            }
            catch
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Converts all slashes and backslashes to OS-specific directory separator characters. Useful for sanitising user input.
        /// </summary>
        public static string PathSanitise(string path)
        {
            return path.Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar).TrimEnd(Path.DirectorySeparatorChar);
        }

        /// <summary>
        /// Converts all OS-specific directory separator characters to '/'. Useful for outputting to a config file or similar.
        /// </summary>
        public static string PathStandardise(string path)
        {
            return path.Replace(Path.DirectorySeparatorChar, '/');
        }

        [Flags]
        public enum MoveFileFlags
        {
            None = 0,
            ReplaceExisting = 1,
            CopyAllowed = 2,
            DelayUntilReboot = 4,
            WriteThrough = 8,
            CreateHardlink = 16,
            FailIfNotTrackable = 32,
        }

        public static class NativeMethods
        {
            [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            public static extern bool MoveFileEx(
                string lpExistingFileName,
                string lpNewFileName,
                MoveFileFlags dwFlags);
        }

        public static bool FileDeleteOnReboot(string filename)
        {
            filename = PathSanitise(filename);

            try
            {
                File.Delete(filename);
                return true;
            }
            catch { }

            string deathLocation = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            try
            {
                File.Move(filename, deathLocation);
            }
            catch
            {
                deathLocation = filename;
            }

            return NativeMethods.MoveFileEx(deathLocation, null, MoveFileFlags.DelayUntilReboot);
        }

        public static bool FileDelete(string filename)
        {
            filename = PathSanitise(filename);

            if (!File.Exists(filename)) return true;

            try
            {
                File.Delete(filename);
                return true;
            }
            catch { }

            try
            {
                //try alternative method: move to a cleanup folder and delete later.
                if (!Directory.Exists(@"_cleanup"))
                {
                    DirectoryInfo di = Directory.CreateDirectory(@"_cleanup");
                    di.Attributes |= FileAttributes.Hidden;
                }

                File.Move(filename, CLEANUP_DIRECTORY + @"/" + Guid.NewGuid());
                return true;
            }
            catch { }

            return false;
        }

        public static string AsciiOnly(string input)
        {
            if (input == null) return null;

            StringBuilder asc = new StringBuilder(input.Length);
            //keep only ascii chars
            foreach (char c in input)
                if (c <= 126)
                    asc.Append(c);
            return asc.ToString().Trim();
        }

        public static void RecursiveMove(string oldDirectory, string newDirectory)
        {
            oldDirectory = PathSanitise(oldDirectory);
            newDirectory = PathSanitise(newDirectory);

            if (oldDirectory == newDirectory)
                return;

            foreach (string dir in Directory.GetDirectories(oldDirectory))
            {
                string newSubDirectory = dir;
                newSubDirectory = Path.Combine(newDirectory, newSubDirectory.Remove(0, 1 + newSubDirectory.LastIndexOf(Path.DirectorySeparatorChar)));

                try
                {
                    DirectoryInfo newDirectoryInfo = Directory.CreateDirectory(newSubDirectory);

                    if ((new DirectoryInfo(dir).Attributes & FileAttributes.Hidden) > 0)
                        newDirectoryInfo.Attributes |= FileAttributes.Hidden;
                }
                catch { }

                RecursiveMove(dir, newSubDirectory);
            }

            bool didExist = Directory.Exists(newDirectory);
            if (!didExist)
            {
                DirectoryInfo newDirectoryInfo = Directory.CreateDirectory(newDirectory);
                try
                {
                    if ((new DirectoryInfo(oldDirectory).Attributes & FileAttributes.Hidden) > 0)
                        newDirectoryInfo.Attributes |= FileAttributes.Hidden;
                }
                catch { }
            }

            foreach (string file in Directory.GetFiles(oldDirectory))
            {
                string newFile = Path.Combine(newDirectory, Path.GetFileName(file));

                bool didMove = FileMove(file, newFile, didExist);
                if (!didMove)
                {
                    try
                    {
                        File.Copy(file, newFile);
                    }
                    catch { }
                    File.Delete(file);
                }

            }

            Directory.Delete(oldDirectory, true);
        }

        public static string GetExtension(string filename)
        {
            return Path.GetExtension(filename).Trim('.').ToLower();
        }

        public static int GetMaxPathLength(string directory)
        {
            int highestPathLength = directory.Length;
            int tempPathLength;

            foreach (string file in Directory.GetFiles(directory))
            {
                if (file.Length > highestPathLength)
                    highestPathLength = file.Length;
            }

            foreach (string dir in Directory.GetDirectories(directory))
            {
                tempPathLength = GetMaxPathLength(dir);
                if (tempPathLength > highestPathLength)
                    highestPathLength = tempPathLength;
            }

            return highestPathLength;
        }

        public static string CleanStoryboardFilename(string filename)
        {
            return PathStandardise(filename.Trim(new[] { '"' }));
        }

        public static byte[] StringToByteArray(String hex)
        {
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }

        public static TDest[] ConvertArray<TSource, TDest>(TSource[] source)
            where TSource : struct
            where TDest : struct
        {

            if (source == null)
                throw new ArgumentNullException("source");

            var sourceType = typeof(TSource);
            var destType = typeof(TDest);

            if (sourceType == typeof(char) || destType == typeof(char))
                throw new NotSupportedException(
                    "Can not convert from/to a char array. Char is special " +
                    "in a somewhat unknown way (like enums can't be based on " +
                    "char either), and Marshal.SizeOf returns 1 even when the " +
                    "values held by a char can be above 255."
                );

            var sourceByteSize = Buffer.ByteLength(source);
            var destTypeSize = Marshal.SizeOf(destType);
            if (sourceByteSize % destTypeSize != 0)
                throw new Exception(
                    "The source array is " + sourceByteSize + " bytes, which can " +
                    "not be transfered to chunks of " + destTypeSize + ", the size " +
                    "of type " + typeof(TDest).Name + ". Change destination type or " +
                    "pad the source array with additional values."
                );

            var destCount = sourceByteSize / destTypeSize;
            var destArray = new TDest[destCount];

            Buffer.BlockCopy(source, 0, destArray, 0, sourceByteSize);

            return destArray;
        }

        public static void CreateBackup(string filename)
        {
            string backupFilename = filename + @"." + DateTime.Now.Ticks + @".bak";
            if (File.Exists(filename) && !File.Exists(backupFilename))
            {
                // Debug.Log(@"Backup created: " + backupFilename);
                File.Move(filename, backupFilename);
            }
        }

        public static string GetRelativePath(string path, string folder)
        {
            path = PathStandardise(path).TrimEnd('/');
            folder = PathStandardise(folder).TrimEnd('/');

            if (path.Length < folder.Length + 1 || path[folder.Length] != '/' || !path.StartsWith(folder))
                throw new ArgumentException(path + " isn't contained in " + folder);

            return path.Substring(folder.Length + 1);
        }

        public static string GetTempPath(string suffix = "")
        {
            string directory = Path.Combine(Path.GetTempPath(), @"osu!");
            Directory.CreateDirectory(directory);
            return Path.Combine(directory, suffix);
        }

        /// <summary>
        /// Returns the path without the extension of the file. 
        /// Contrarily to Path.GetFileNameWithoutExtension, it keeps the path to the file ("sb/triangle.png" becomes "sb/triangle" and not "triangle")
        /// </summary>
        public static string StripExtension(string filepath)
        {
            int dotIndex = filepath.LastIndexOf('.');
            return dotIndex == -1 ? filepath : filepath.Substring(0, dotIndex);
        }

        public static string FormatHeader(string content, int screenWidth = 80)
        {
            int halfWidth = Convert.ToInt32(Math.Floor(screenWidth / 2.0f));
            int padcount = Convert.ToInt32(Math.Round(content.Length / 2.0f));
            string res = content.PadLeft(halfWidth + padcount, '=');
            res = res.PadRight(screenWidth, '=');
            return res;
        }

        public static string Base64Encode(string data)
        {
            var bytes = Encoding.UTF8.GetBytes(data);
            return Convert.ToBase64String(bytes);
        }
        public static string Base64Decode(string base64Data)
        {
            var encodedBytes = Convert.FromBase64String(base64Data);
            return Encoding.UTF8.GetString(encodedBytes);
        }
    }
}
