using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Reflection;

#if MONO
using Mono.Npgsql;
#else
using Npgsql;
#endif //MONO

using ObjectServer.Model;

namespace ObjectServer.Backend.Postgresql
{
    internal sealed class PgTableContext : ITableContext
    {
        private IDictionary<string, IColumnMetadata> columns = new Dictionary<string, IColumnMetadata>();
        private readonly IDictionary<OnDeleteAction, string> onDeleteMapping =
            new Dictionary<OnDeleteAction, string>()
            {
                { OnDeleteAction.SetNull, "SET NULL" },
                { OnDeleteAction.SetDefault, "SET DEFAULT" },
                { OnDeleteAction.Cascade, "CASCADE" },
                { OnDeleteAction.NoAction, "NO ACTION" },
                { OnDeleteAction.Restrict, "RESTRICT" },
            };

        public PgTableContext(IDataContext db, string tableName)
        {
            this.Name = tableName;

            this.LoadColumns(db, tableName);
        }

        public string Name { get; private set; }

        public bool TableExists(IDataContext db, string tableName)
        {
            //检查连接
            var sql = @"
    SELECT COALESCE(COUNT(table_name), 0)
        FROM information_schema.tables 
        WHERE table_type = 'BASE TABLE' AND table_schema = 'public' AND table_name = @0
";

            var n = (long)db.QueryValue(sql, tableName);
            return n > 0;
        }

        public void CreateTable(IDataContext db, string tableName, string label)
        {
            //TODO SQL 注入风险
            var sql = string.Format(
                @"CREATE TABLE ""{0}"" (id BIGSERIAL NOT NULL, PRIMARY KEY(id)) WITHOUT OIDS",
                tableName);
            db.Execute(sql);
            sql = string.Format(
                @"COMMENT ON TABLE ""{0}"" IS '{1}';",
                tableName, label);
            db.Execute(sql);
        }

        public void AddColumn(IDataContext db, IMetaField field)
        {
            var sqlType = PgSqlTypeConverter.GetSqlType(field);
            var notNull = field.IsRequired ? "NOT NULL" : "";

            var sql = string.Format(
                @"ALTER TABLE ""{0}"" ADD COLUMN ""{1}"" {2} {3}",
                this.Name, field.Name, sqlType, notNull);
            db.Execute(sql);

            sql = string.Format(
                "COMMENT ON COLUMN \"{0}\".\"{1}\" IS '{2}'",
                this.Name, field.Name, field.Label);
            db.Execute(sql);
        }

        public void DeleteColumn(IDataContext db, string columnName)
        {
            var sql = string.Format(
                "ALTER TABLE \"{0}\" DROP COLUMN \"{1}\"",
                this.Name, columnName);

            db.Execute(columnName);
        }

        public void AlterColumnNullable(IDataContext db, string columnName, bool nullable)
        {
            var action = nullable ? "DROP NOT NULL" : "SET NOT NULL";
            var sql = string.Format(
                "ALTER TABLE \"{0}\"ALTER COLUMN \"{1}\" {2}",
                this.Name, columnName, action);
            db.Execute(sql);
        }

        public void AlterColumnType(IDataContext db, string columnName, string sqlType)
        {
            var sql = string.Format(
                "ALTER TABLE \"{0}\"ALTER \"{1}\" TYPE {2}",
                this.Name, columnName, sqlType);
            db.Execute(sql);
        }

        public bool ColumnExists(string columnName)
        {
            return this.columns.ContainsKey(columnName);
        }

        public IColumnMetadata GetColumn(string columnName)
        {
            return this.columns[columnName];
        }

        public IColumnMetadata[] GetAllColumns()
        {
            return this.columns.Values.ToArray();
        }

        private void LoadColumns(IDataContext db, string tableName)
        {
            var sql = @"
SELECT column_name, data_type, is_nullable
    FROM information_schema.columns
    WHERE table_name = @0
    ORDER BY ordinal_position;
";
            var records = db.QueryAsDictionary(sql, tableName);
            this.columns.Clear();
            foreach (var r in records)
            {
                var column = new PgColumnMetadata(r);
                this.columns.Add(column.Name, column);
            }

        }


        public void AddConstraint(IDataContext db, string constraintName, string constraint)
        {
            throw new NotImplementedException();
        }


        public void DeleteConstraint(IDataContext db, string constraintName)
        {
            var sql = string.Format(
                "alter table \"{0}\" drop constraint \"{1}\"",
                this.Name, constraintName);
            db.Execute(sql);
        }


        public void AddFK(IDataContext db, string columnName, string refTable, OnDeleteAction act)
        {
            var onDelete = onDeleteMapping[act];

            var fkName = this.GenerateFkName(columnName);
            var sql = string.Format(
                "ALTER TABLE \"{0}\" ADD CONSTRAINT \"{1}\" FOREIGN KEY (\"{2}\") REFERENCES \"{3}\" ON DELETE {4}",
                this.Name, fkName, columnName, refTable, onDelete);

            db.Execute(sql);
        }


        public void DeleteFK(IDataContext db, string columnName)
        {
            var fkName = this.GenerateFkName(columnName);
            this.DeleteConstraint(db, fkName);
        }

        public bool FKExists(IDataContext db, string columnName)
        {
            var sql = @"
SELECT COALESCE(COUNT(constraint_name), 0)
    FROM information_schema.key_column_usage 
    WHERE constraint_schema = 'public' AND table_name = @0 AND column_name = @1
";
            var n = (long)db.QueryValue(sql, this.Name, columnName);
            return n > 0;
        }

        private string GenerateFkName(string columnName)
        {
            return string.Format("{0}_{1}_fkey", this.Name, columnName);
        }
    }
}
