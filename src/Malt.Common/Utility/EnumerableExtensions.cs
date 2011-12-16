using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;

namespace Malt.Utility
{
    public static class EnumerableExtensions
    {
        public static string ToCsv(this IEnumerable items)
        {
            if (items == null)
            {
                throw new ArgumentNullException("items");
            }

            var sb = new StringBuilder();
            var flag = true;
            foreach (var item in items)
            {
                if (flag)
                {
                    flag = false;
                }
                else
                {
                    sb.Append(",");
                }

                sb.Append(item.ToString());
            }

            return sb.ToString();
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
