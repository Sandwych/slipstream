using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Reflection;

using Npgsql;

using ObjectServer.Model;

namespace ObjectServer.Backend
{
    internal sealed class PgTableHandler : ITableHandler
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(
            MethodBase.GetCurrentMethod().DeclaringType);

        private IDatabase db;

        public PgTableHandler(IDatabase db, string tableName)
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

        public void AddColumn(IField field)
        {
            var sqlType = PgSqlTypeConverter.GetSqlType(field);
            var notNull = field.Required ? "not null" : "";

            var sql = string.Format(
                @"ALTER TABLE ""{0}"" ADD COLUMN ""{1}"" {2} {3}",
                this.Name, field.Name, sqlType, notNull);
            this.db.Execute(sql);

            sql = string.Format(
                "comment on column \"{0}\".\"{1}\" IS '{2}'",
                this.Name, field.Name, field.Name);
            this.db.Execute(sql);
        }

        public void UpgradeColumn(IField field)
        {
            throw new NotImplementedException();
        }
    }
}
