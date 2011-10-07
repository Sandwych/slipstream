using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NHibernate.SqlCommand;

namespace ObjectServer.Model
{
    public static class SqlStringExtensions
    {
        public static SqlString JoinBy(this IEnumerable<SqlString> items, string junction)
        {
            if (items == null)
            {
                throw new ArgumentNullException("items");
            }

            if (string.IsNullOrEmpty(junction))
            {
                throw new ArgumentNullException("junction");
            }

            var sb = new SqlStringBuilder();
            var flag = true;
            foreach (var item in items)
            {
                if (flag)
                {
                    flag = false;
                }
                else
                {
                    sb.Add(" ");
                    sb.Add(junction);
                    sb.Add(" ");
                }

                sb.Add("(");
                sb.Add(item);
                sb.Add(")");
            }

            return sb.ToSqlString();
        }

        public static SqlString JoinByAnd(this IEnumerable<SqlString> items)
        {
            return items.JoinBy("and");
        }

        public static SqlString JoinByOr(this IEnumerable<SqlString> items)
        {
            return items.JoinBy("and");
        }

    }
}
