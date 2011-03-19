using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.Utility
{
    public static class EnumerableExtensions
    {

        public static string ToCommaList<T>(this IEnumerable<T> items)
        {
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
    }
}
