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
    internal sealed class PgTableContext : ITableContext
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(
            MethodBase.GetCurrentMethod().DeclaringType);

        private List<Column> columns = new List<Column>();

        public PgTableContext(IDatabaseContext db, string tableName)
        {
            this.Name = tableName;

            this.LoadColumns();
        }

        public string Name { get; private set; }

        public bool TableExists(IDatabaseContext db, string tableName)
        {
            //检查连接
            var sql =
@"
select count(relname) from pg_class 
    where relkind IN ('r','v') and relname=@0";

            var n = (long)db.QueryValue(sql, tableName);
            return n > 0;
        }

        public void CreateTable(IDatabaseContext db, string tableName, string label)
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

        public void AddColumn(IDatabaseContext db, IMetaField field)
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

        public void DeleteColumn(IDatabaseContext db, string columnName)
        {
            var sql = string.Format(
                "alter table \"{0}\" drop column \"{1}\"",
                this.Name, columnName);

            db.Execute(columnName);
        }

        public void UpgradeColumn(IDatabaseContext db, IMetaField field)
        {
            throw new NotImplementedException();
        }

        private void LoadColumns()
        {
        }


        public void AddConstraint(IDatabaseContext db, string constraintName, string constraint)
        {
            throw new NotImplementedException();
        }


        public void DeleteConstraint(IDatabaseContext db, string constraintName)
        {
            var sql = string.Format(
                "alter table \"{0}\" drop constraint \"{1}\"",
                this.Name, constraintName);
            db.Execute(sql);
        }


        public void AddFk(IDatabaseContext db, string columnName, string refTable, ReferentialAction refAct)
        {
            //TODO: 设置其它的 ReferentialAction
            var onDelete = "set null";

            var fkName = this.GenerateFkName(columnName);
            var sql = string.Format(
                "alter table \"{0}\" add constraint \"{1}\" foreign key (\"{2}\") references \"{3}\" on delete {4}",
                this.Name, fkName, columnName, refTable, onDelete);

            db.Execute(sql);
        }


        public void DeleteFk(IDatabaseContext db, string columnName)
        {
            var fkName = this.GenerateFkName(columnName);
            this.DeleteConstraint(db, fkName);
        }

        public bool FkExists(IDatabaseContext db, string columnName)
        {
            var fkName = this.GenerateFkName(columnName);
            var sql = "select count(conname) from pg_constraint where conname = @0";
            var n = (long)db.QueryValue(sql, fkName);
            return n > 0;
        }

        private string GenerateFkName(string columnName)
        {
            return string.Format("{0}_{1}_fkey", this.Name, columnName);
        }
    }
}
