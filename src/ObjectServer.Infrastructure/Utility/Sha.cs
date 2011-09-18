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

            using (var sha = System.Security.Cryptography.SHA1.Create())
            {
                var bytes = Encoding.ASCII.GetBytes(value);                
                var hash = sha.ComputeHash(bytes);
                return hash.ToHex();
            }
        }

    }
}
