using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Malt.Utility
{
    public static class Sha
    {
        /// <summary>
        /// 计算 SHA1 哈希值，并转换成64位16进制字符串
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static string ToSha(this byte[] buffer)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }

            using (var sha = new System.Security.Cryptography.SHA1Managed())
            {
                var hash = sha.ComputeHash(buffer);
                return hash.ToHex();
            }
        }

        /// <summary>
        /// 计算 UTF-8 字符串的 SHA1 哈希值，并转换成64位16进制字符串
        /// </summary>
        /// <param name="value">待计算的 ASCII 字符串</param>
        /// <returns></returns>
        public static string ToSha(this string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentNullException("value");
            }

            var bytes = Encoding.UTF8.GetBytes(value);
            return bytes.ToSha();
        }

    }
}
