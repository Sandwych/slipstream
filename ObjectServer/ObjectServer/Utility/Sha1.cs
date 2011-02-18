using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.Utility
{
    public static class Sha1
    {
        public static string ToSha1(this string value)
        {
            using (var sha = System.Security.Cryptography.SHA1.Create())
            {
                var encoder = new ASCIIEncoding();
                var bytes = encoder.GetBytes(value);
                var hash = sha.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }
    }
}
