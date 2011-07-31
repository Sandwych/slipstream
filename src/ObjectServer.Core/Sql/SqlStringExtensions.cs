using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NHibernate.SqlCommand;

namespace ObjectServer.Sql
{
    public static class SqlStringExtensions
    {
        public static SqlString JoinByAnd(this IEnumerable<SqlString> items)
        {
            if (items == null)
            {
                throw new ArgumentNullException("items");
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
                    sb.Add(" and ");
                }

                sb.Add("(");
                sb.Add(item);
                sb.Add(")");
            }

            return sb.ToSqlString();
        }
    }
}
