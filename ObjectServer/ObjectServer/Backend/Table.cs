using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

using Npgsql;

namespace ObjectServer.Backend
{
    public class Table
    {
        public bool TableExists(IDbConnection conn, string tableName)
        {
            //检查连接
            var sql = string.Format(
                "select count(relname) from pg_class " +
                "   where relkind IN ('r','v') and relname='{0}'",
                tableName);

            var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            var n = (long)cmd.ExecuteScalar();
            return n > 0;
        }

        public void CreateTable(IDbConnection conn, string tableName, string label)
        {
            //TODO SQL 注入风险
            var sql = string.Format(
                @"CREATE TABLE ""{0}"" (id BIGSERIAL NOT NULL, PRIMARY KEY(id)) WITHOUT OIDS;",
                tableName);
            conn.ExecuteNonQuery(sql);
            sql = string.Format(
                @"COMMENT ON TABLE ""{0}"" IS '{1}';",
                tableName, label);
            conn.ExecuteNonQuery(sql);

        }

        public void AddColumn(IDbConnection conn, string tableName, string colName, string sqlType)
        {
            var sql = string.Format(
                @"ALTER TABLE ""{0}"" ADD COLUMN ""{1}"" {2}",
                tableName, colName, sqlType);
            conn.ExecuteNonQuery(sql);
        }
    }
}
