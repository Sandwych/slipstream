using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;

namespace ObjectServer.Backend
{
    public static class DataExtensions
    {

        public static int ExecuteNonQuery(this IDbConnection conn, string sql)
        {
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = sql;
                return cmd.ExecuteNonQuery();
            }
        }

        public static object ExecuteScalar(this IDbConnection conn, string sql)
        {
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = sql;
                return cmd.ExecuteScalar();
            }
        }

        public static string EscapeSqlString(this string str)
        {
            return str.Replace("'", "''");
        }

    }
}
