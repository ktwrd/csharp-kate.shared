using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Numerics;
using System.Threading.Tasks;
using System.Reflection;

namespace kate.shared.Helpers
{
    public static class GeneralHelper
    {
        /// <summary>
        /// Format the duration between <paramref name="start"/> and the current date (using <see cref="DateTimeOffset.UtcNow"/>
        /// </summary>
        /// <returns><see cref="FormatDuration(TimeSpan)"/></returns>
        public static string GenerateTaskDuration(DateTimeOffset start)
        {
            var end = DateTimeOffset.UtcNow;
            var diff = end - start;

            return FormatDuration(diff);
        }
        /// <summary>
        /// Will format a timespan to `HH:MM:ss.sss` or `ss.sss seconds`.
        /// </summary>
        public static string FormatDuration(TimeSpan duration)
        {
            var st = new List<string>();

            if (duration.Hours > 0)
                st.Add(duration.Hours.ToString().PadLeft(2, '0'));
            if (duration.Minutes > 0)
                st.Add(duration.Minutes.ToString().PadLeft(2, '0'));

            st.Add(duration.Seconds.ToString().PadLeft(2, '0') + "." + duration.Milliseconds.ToString().PadLeft(3, '0'));

            var stj = string.Join(":", st);
            if (st.Count < 2)
            {
                return $"{stj} seconds";
            }
            else
            {
                return stj;
            }
        }
        /// <summary>
        /// Start all tasks in <paramref name="taskList"/> then return <see cref="Task.WhenAll(IEnumerable{Task})"/> where the parameter is the taskList provided.
        /// </summary>
        public static Task WaitTaskList(IEnumerable<Task> taskList)
        {
            foreach (var i in taskList)
                i.Start();
            return Task.WhenAll(taskList);
        }
        /// <summary>
        /// Fetch an embedded resource from the assembly provided.
        /// </summary>
        /// <returns>null when not found</returns>
        public static string GetEmbeddedResourceAsString(string resourceName, Assembly assembly)
        {
            string data = null;
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream != null)
                {
                    using (var reader = new StreamReader(stream))
                    {
                        data = reader.ReadToEnd();
                    }
                }
            }
            return data;
        }
        /// <summary>
        /// Check if the embedded resource in <paramref name="assembly"/> exists.
        /// </summary>
        /// <param name="resourceName">Name of the resource. Example; <code>SQLTool.Shared.Scripts.DeIdentify_TMS_Drop.sql</code></param>
        /// <param name="assembly">Assembly to search for the Resource in.</param>
        /// <returns><see langword="true"/> when it does exist, <see langword="false"/> when it couldn't be found.</returns>
        public static bool EmbeddedResourceExists(string resourceName, Assembly assembly)
        {
            if (assembly == null)
                return false;
            var names = assembly.GetManifestResourceNames();
            foreach (var x in names)
            {
                if (x == resourceName)
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// Convert an array of bytes to hexadecimal.
        /// </summary>
        public static string ToHex(byte[] data)
        {
            return BitConverter.ToString(data).Replace("-", "");
        }
        /// <summary>
        /// Create a SHA256 based off the <paramref name="content"/> provided.
        /// </summary>
        /// <param name="content">Content to hash</param>
        /// <returns>Uppercase SHA256 Hash in Hexadecimal.</returns>
        public static string CreateSha256Hash(byte[] content)
        {
            using (var ms = new MemoryStream(content))
            {
                return CreateSha256Hash(ms);
            }
        }
        /// <inheritdoc cref="CreateSha256Hash(byte[])"/>
        public static string CreateSha256Hash(Stream content)
        {
            using (SHA256 hash = SHA256.Create())
            {
                var bytes = hash.ComputeHash(content);
                var builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString().ToUpper();
            }
        }
        /// <inheritdoc cref="CreateSha256Hash(byte[])"/>
        public static string CreateSha256Hash(string content)
        {
            return CreateSha256Hash(Encoding.UTF8.GetBytes(content));
        }
        /// <summary>
        /// Get all types that have the attribute provided.
        /// </summary>
        /// <param name="asm">Assembly to use.</param>
        public static IEnumerable<(Type, TAttribute)> GetTypesByAttribute<TAttribute>(Assembly asm)
            where TAttribute : Attribute
        {
            foreach (Type type in asm.GetTypes())
            {
                var tr = type.GetCustomAttribute<TAttribute>(true);
                if (tr != null)
                {
                    yield return (type, tr);
                }
            }
        }

        /// <summary>
        /// Allocates a new console for the calling process.
        /// </summary>
        /// <returns>
        /// When `false`, the function has failed. You can call <see href="https://learn.microsoft.com/en-us/windows/win32/api/errhandlingapi/nf-errhandlingapi-getlasterror">GetLastError</see> to get more details.</returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool AllocConsole();

        /// <summary>
        /// Properly format a strings line endings to the desired line endings. Currently supports; LF, CR, and CRLF.
        /// </summary>
        /// <param name="content">Content to properly format the line endings.</param>
        /// <param name="lineEnding">Target line ending. Defaulted to LF</param>
        /// <returns>String with the properly formatted line endings.</returns>
        public static string FormatLineEndings(string content, string lineEnding="\n")
        {
            var lines = new List<string>();
            string currentWorkingString = "";

            char previousChar = (char)0;
            char LF = (char)10;
            char CR = (char)13;
            for (int i = 0; i < content.Length; i++)
            {
                char c = content[i];
                // not any form of line ending, which means
                // it's safe to append to working string.
                if (c != CR && c != LF)
                {
                    currentWorkingString += c;
                }

                // current char is LF and previous isn't CR
                // that way we know it's just LF line endings
                // and not CRLF line endings
                if (c == LF && previousChar != CR)
                {
                    lines.Add(currentWorkingString);
                    currentWorkingString = "";
                }

                // always safe to add CR line endings
                // since in all common line endings,
                // CR is before LF or LF doesn't exist
                // at all so it's just CR.
                if (c == CR)
                {
                    lines.Add(currentWorkingString);
                    currentWorkingString = "";
                }
                previousChar = c;
            }
            lines.Add(currentWorkingString);

            return string.Join(lineEnding, lines);
        }
        /// <summary>
        /// Trim an Array to make sure it's length doesn't exceed the <paramref name="length"/> provided.
        /// </summary>
        /// <typeparam name="T">Type of the items in the <paramref name="array"/></typeparam>
        /// <param name="array">Array of items with the type of <typeparamref name="T"/> that you want to trim.</param>
        /// <param name="length">Maximum length of result.</param>
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
        /// <summary>
        /// Get a list of all items that are in the type provided.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<T> GetEnumList<T>()
        {
            T[] array = (T[])Enum.GetValues(typeof(T));
            List<T> list = new List<T>(array);
            return list;
        }
        /// <summary>
        /// Write the string <paramref name="content"/> as UTF8 to the <paramref name="stream"/> provided.
        /// </summary>
        public static void WriteStringToMemoryStream(MemoryStream stream, string content, int position=0)
        {
            var bytes = Encoding.UTF8.GetBytes(content);
            stream.Write(bytes, position, bytes.Length);
        }
        /// <summary>
        /// Get nanoseconds from <see cref="Stopwatch.GetTimestamp"/>
        /// </summary>
        public static long GetNanoseconds()
        {
            double timestamp = Stopwatch.GetTimestamp();
            double nanoseconds = 1_000_000_000.0 * timestamp / Stopwatch.Frequency;

            return (long)nanoseconds;
        }
        /// <summary>
        /// Get microseconds from <see cref="Stopwatch.GetTimestamp"/>
        /// </summary>
        /// <returns></returns>
        public static long GetMicroseconds()
        {
            double timestamp = Stopwatch.GetTimestamp();
            double microseconds = 1_000_000.0 * timestamp / Stopwatch.Frequency;
            return (long)microseconds;
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
        /// <summary>
        /// Generate a string that is all capitals from A-Z and 0-9.
        /// </summary>
        /// <param name="length">Length of the token to generate.</param>
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
        /// <summary>
        /// <para>Generate a string that can be used as a unique identifier.</para>
        ///
        /// <para>A-Z Uppercase, A-Z lowercase, and 0-9.</para>
        /// </summary>
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
        /// <summary>
        /// Generate a GUID with <see cref="Guid.NewGuid"/>
        /// </summary>
        public static string GenerateGUID()
        {
            return Guid.NewGuid().ToString();
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

        /// <summary>
        /// Encoding the provided value to be suitable for a URL Parameter.
        /// </summary>
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

        /// <summary>
        /// Get the extension of a file.
        /// </summary>
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
        /// <summary>
        /// Convert a string to a byte array.
        /// </summary>
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

        /// <summary>
        /// Encode a string into Base64 (UTF8)
        /// </summary>
        public static string Base64Encode(string data)
        {
            var bytes = Encoding.UTF8.GetBytes(data);
            return Convert.ToBase64String(bytes);
        }
        /// <summary>
        /// Decode a Base64 UTF8 encoded string
        /// </summary>
        public static string Base64Decode(string base64Data)
        {
            var encodedBytes = Convert.FromBase64String(base64Data);
            return Encoding.UTF8.GetString(encodedBytes);
        }
    }
}
