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

        private Database db;

        public Table(Database db, string tableName)
        {
            this.db = db;
            this.Name = tableName;
        }

        public string Name { get; private set; }

        public bool TableExists(string tableName)
        {
            //检查连接
            var sql =
@"
select count(relname) from pg_class 
    where relkind IN ('r','v') and relname=@0";

            var n = (long)this.db.QueryValue(sql, tableName);
            return n > 0;
        }

        public void CreateTable(string tableName, string label)
        {
            //TODO SQL 注入风险
            var sql = string.Format(
                @"CREATE TABLE ""{0}"" (id BIGSERIAL NOT NULL, PRIMARY KEY(id)) WITHOUT OIDS;",
                tableName);
            this.db.Execute(sql);
            sql = string.Format(
                @"COMMENT ON TABLE ""{0}"" IS '{1}';",
                tableName, label);
            this.db.Execute(sql);
        }

        public void AddColumn(string colName, string sqlType)
        {
            //TODO: 目前只支持空表，如果有数据的话就涉及迁移了
            var sql = string.Format(
                @"ALTER TABLE ""{0}"" ADD COLUMN ""{1}"" {2}",
                this.Name, colName, sqlType);
            this.db.Execute(sql);
        }

        public long NextSerial(string sequenceName)
        {
            var seqSql = "SELECT nextval(@0)";
            var serial = (long)this.db.QueryValue(seqSql, sequenceName);
            return serial;
        }
    }
}
