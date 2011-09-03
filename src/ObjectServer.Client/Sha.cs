using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.Utility
{
    public static class Sha
    {
        /// <summary>
        /// 计算 SHA1 哈希值，并转换成64位16进制字符串
        /// </summary>
        /// <param name="value">待计算的 ASCII 字符串</param>
        /// <returns></returns>
        public static string ToSha(this string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentNullException("value");
            }

            using (var sha = new System.Security.Cryptography.SHA1Managed())
            {
                var bytes = Encoding.UTF8.GetBytes(value);                
                var hash = sha.ComputeHash(bytes);
                return hash.ToHex();
            }
        }

        public static string ToHex(this byte[] bytes)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException("bytes");
            }

            const string HexChars = "0123456789ABCDEF";
            var sb = new StringBuilder(bytes.Length * 2);
            foreach (var b in bytes)
            {
                sb.Append(HexChars[b / 16]);
                sb.Append(HexChars[b % 16]);
            }
            return sb.ToString();
        }

    }
}
