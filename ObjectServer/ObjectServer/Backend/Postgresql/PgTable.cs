using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Reflection;

using Npgsql;

using ObjectServer.Model;

namespace ObjectServer.Backend.Postgresql
{
    internal sealed class PgTable : ITable
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(
            MethodBase.GetCurrentMethod().DeclaringType);

        private List<Column> columns = new List<Column>();

        public PgTable(IDatabase db, string tableName)
        {
            this.Name = tableName;

            this.LoadColumns();
        }

        public string Name { get; private set; }

        public bool TableExists(IDatabase db, string tableName)
        {
            //检查连接
            var sql =
@"
select count(relname) from pg_class 
    where relkind IN ('r','v') and relname=@0";

            var n = (long)db.QueryValue(sql, tableName);
            return n > 0;
        }

        public void CreateTable(IDatabase db, string tableName, string label)
        {
            //TODO SQL 注入风险
            var sql = string.Format(
                @"CREATE TABLE ""{0}"" (id BIGSERIAL NOT NULL, PRIMARY KEY(id)) WITHOUT OIDS;",
                tableName);
            db.Execute(sql);
            sql = string.Format(
                @"COMMENT ON TABLE ""{0}"" IS '{1}';",
                tableName, label);
            db.Execute(sql);
        }

        public void AddColumn(IDatabase db, IField field)
        {
            var sqlType = PgSqlTypeConverter.GetSqlType(field);
            var notNull = field.Required ? "not null" : "";

            var sql = string.Format(
                @"ALTER TABLE ""{0}"" ADD COLUMN ""{1}"" {2} {3}",
                this.Name, field.Name, sqlType, notNull);
            db.Execute(sql);

            sql = string.Format(
                "comment on column \"{0}\".\"{1}\" IS '{2}'",
                this.Name, field.Name, field.Name);
            db.Execute(sql);
        }

        public void DeleteColumn(IDatabase db, string columnName)
        {
            var sql = string.Format(
                "alter table \"{0}\" drop column \"{1}\"",
                this.Name, columnName);

            db.Execute(columnName);
        }

        public void UpgradeColumn(IDatabase db, IField field)
        {
            throw new NotImplementedException();
        }

        private void LoadColumns()
        {
        }
    }
}
