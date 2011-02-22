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

        private Dictionary<string, Column> columns = new Dictionary<string, Column>();

        public PgTableContext(IDatabaseContext db, string tableName)
        {
            this.Name = tableName;

            this.LoadColumns(db, tableName);
        }

        public string Name { get; private set; }

        public bool TableExists(IDatabaseContext db, string tableName)
        {
            //检查连接
            var sql =
                "select count(relname) from pg_class where relkind IN ('r','v') and relname = @0";

            var n = (long)db.QueryValue(sql, tableName);
            return n > 0;
        }

        public void CreateTable(IDatabaseContext db, string tableName, string label)
        {
            //TODO SQL 注入风险
            var sql = string.Format(
                @"create table ""{0}"" (id bigserial not null, primary key(id)) without oids",
                tableName);
            db.Execute(sql);
            sql = string.Format(
                @"comment on table ""{0}"" is '{1}';",
                tableName, label);
            db.Execute(sql);
        }

        public void AddColumn(IDatabaseContext db, IMetaField field)
        {
            var sqlType = PgSqlTypeConverter.GetSqlType(field);
            var notNull = field.Required ? "not null" : "";

            var sql = string.Format(
                @"alter table ""{0}"" add column ""{1}"" {2} {3}",
                this.Name, field.Name, sqlType, notNull);
            db.Execute(sql);

            sql = string.Format(
                "comment on column \"{0}\".\"{1}\" is '{2}'",
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

        public bool ColumnExists(string columnName)
        {
            return this.columns.ContainsKey(columnName);
        }

        private void LoadColumns(IDatabaseContext db, string tableName)
        {
            var sql = @"
select column_name, data_type, is_nullable <> 'NO' as not_null
    from information_schema.columns
    where table_name = @0
    order by ordinal_position;
";
            var records = db.QueryAsDictionary(sql, tableName);
            this.columns.Clear();
            foreach (var r in records)
            {
                var column = new Column()
                {
                    Name = (string)r["column_name"],
                    NotNull = (bool)r["not_null"],
                    SqlType = (string)r["data_type"],
                };
                this.columns.Add(column.Name, column);
            }

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


        public void AddFK(IDatabaseContext db, string columnName, string refTable, ReferentialAction refAct)
        {
            //TODO: 设置其它的 ReferentialAction
            var onDelete = "set null";

            var fkName = this.GenerateFkName(columnName);
            var sql = string.Format(
                "alter table \"{0}\" add constraint \"{1}\" foreign key (\"{2}\") references \"{3}\" on delete {4}",
                this.Name, fkName, columnName, refTable, onDelete);

            db.Execute(sql);
        }


        public void DeleteFK(IDatabaseContext db, string columnName)
        {
            var fkName = this.GenerateFkName(columnName);
            this.DeleteConstraint(db, fkName);
        }

        public bool FKExists(IDatabaseContext db, string columnName)
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
