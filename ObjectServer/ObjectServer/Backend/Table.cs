using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Reflection;

using Npgsql;

namespace ObjectServer.Backend
{
    public class Table
    {
        protected static readonly log4net.ILog Log = log4net.LogManager.GetLogger(
            MethodBase.GetCurrentMethod().DeclaringType);

        private IDbConnection conn;

        public Table(IDbConnection conn, string tableName)
        {
            this.conn = conn;
            this.Name = tableName;
        }

        public string Name { get; private set; }

        public bool TableExists(string tableName)
        {
            //检查连接
            var sql = string.Format(
                "select count(relname) from pg_class " +
                "   where relkind IN ('r','v') and relname='{0}'",
                tableName);

            var cmd = this.conn.CreateCommand();
            cmd.CommandText = sql;
            var n = (long)cmd.ExecuteScalar();
            return n > 0;
        }

        public void CreateTable(string tableName, string label)
        {
            //TODO SQL 注入风险
            var sql = string.Format(
                @"CREATE TABLE ""{0}"" (id BIGSERIAL NOT NULL, PRIMARY KEY(id)) WITHOUT OIDS;",
                tableName);
            this.conn.ExecuteNonQuery(sql);
            sql = string.Format(
                @"COMMENT ON TABLE ""{0}"" IS '{1}';",
                tableName, label);
            this.conn.ExecuteNonQuery(sql);

        }

        public void AddColumn(string colName, string sqlType)
        {
            var sql = string.Format(
                @"ALTER TABLE ""{0}"" ADD COLUMN ""{1}"" {2}",
                this.Name, colName, sqlType);
            this.conn.ExecuteNonQuery(sql);
        }

        public long NextSerial(string sequenceName)
        {
            var seqSql = string.Format("SELECT nextval('{0}');",
                sequenceName);

            if (Log.IsDebugEnabled)
            {
                Log.Debug(seqSql);
            }
            var serial = (long)this.conn.ExecuteScalar(seqSql);
            return serial;
        }
    }
}
