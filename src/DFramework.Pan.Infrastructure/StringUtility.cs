using System;
using System.IO;
using System.Security.Cryptography;

namespace DFramework.Pan.Infrastructure
{
    public static class StringUtility
    {
        public static string GetFileMD5(this Stream fileStream)
        {
            var pos = fileStream.Position;
            using (var md5 = MD5.Create())
            {
                var md5String = BytesToHexString(md5.ComputeHash(fileStream));
                if (fileStream.CanSeek)
                {
                    fileStream.Seek(pos, SeekOrigin.Begin);
                }
                return md5String;
            }
        }

        private static readonly uint[] _lookup32 = CreateLookup32();

        private static uint[] CreateLookup32()
        {
            var result = new uint[256];
            for (int i = 0; i < 256; i++)
            {
                string s = i.ToString("X2");
                result[i] = ((uint)s[0]) + ((uint)s[1] << 16);
            }
            return result;
        }

        public static string BytesToHexString(byte[] bytes)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException("bytes");
            }
            var lookup32 = _lookup32;
            var result = new char[bytes.Length * 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                var val = lookup32[bytes[i]];
                result[2 * i] = (char)val;
                result[2 * i + 1] = (char)(val >> 16);
            }
            return new string(result);
        }

        //public static string Encode(this string str)
        //{
        //    return System.Web.HttpUtility.UrlEncode(str);
        //}
    }
}