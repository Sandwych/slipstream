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

            return Encoding.ASCII.GetBytes(value).ToSha();
        }

        public static string ToSha(this byte[] buffer)
        {
            if (buffer == null || buffer.Length == 0)
            {
                throw new ArgumentNullException("buffer");
            }

            using (var sha = System.Security.Cryptography.SHA1.Create())
            {
                var hash = sha.ComputeHash(buffer);
                return hash.ToHex();
            }
        }

    }
}
