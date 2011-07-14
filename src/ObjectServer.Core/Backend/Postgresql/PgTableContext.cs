using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Reflection;
using System.Diagnostics;

#if MONO
using Mono.Npgsql;
#else
using Npgsql;
#endif //MONO

using ObjectServer.Model;
using ObjectServer.Utility;

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

        public PgTableContext(IDBConnection db, string tableName)
        {
            if (db == null)
            {
                throw new ArgumentNullException("db");
            }

            if (string.IsNullOrEmpty(tableName))
            {
                throw new ArgumentNullException("tableName");
            }

            if (!NamingRule.IsValidSqlName(tableName))
            {
                throw new ArgumentOutOfRangeException("tableName");
            }

            this.Name = tableName;
            this.LoadColumns(db, tableName);
        }

        public string Name { get; private set; }

        public bool TableExists(IDBConnection db, string tableName)
        {
            if (db == null)
            {
                throw new ArgumentNullException("db");
            }

            if (tableName == null)
            {
                throw new ArgumentNullException("tableName");
            }

            if (!NamingRule.IsValidSqlName(tableName))
            {
                throw new ArgumentOutOfRangeException("tableName");
            }

            //检查连接
            var sql = @"
    SELECT COALESCE(COUNT(table_name), 0)
        FROM information_schema.tables 
        WHERE table_type = 'BASE TABLE' AND table_schema = 'public' AND table_name = @0
";

            var n = (long)db.QueryValue(sql, tableName);
            return n > 0;
        }

        public void CreateTable(IDBConnection db, string tableName, string label)
        {
            if (db == null)
            {
                throw new ArgumentNullException("db");
            }

            if (string.IsNullOrEmpty(tableName))
            {
                throw new ArgumentNullException("tableName");
            }

            if (!NamingRule.IsValidSqlName(tableName))
            {
                throw new ArgumentOutOfRangeException("tableName");
            }

            tableName = tableName.SqlEscape();
            var sql = string.Format(
                @"CREATE TABLE ""{0}"" (id BIGSERIAL NOT NULL, PRIMARY KEY(""id"")) WITHOUT OIDS",
                tableName.SqlEscape());
            db.Execute(sql);

            label = label.SqlEscape();
            sql = string.Format(
                @"COMMENT ON TABLE ""{0}"" IS '{1}';",
                tableName, label);
            db.Execute(sql);
        }

        public void AddColumn(IDBConnection db, IField field)
        {
            if (db == null)
            {
                throw new ArgumentNullException("db");
            }

            if (field == null)
            {
                throw new ArgumentNullException("field");
            }

            var sqlType = PgSqlTypeConverter.GetSqlType(field);
            var notNull = field.IsRequired ? "NOT NULL" : "";

            var sql = string.Format(
                @"ALTER TABLE ""{0}"" ADD COLUMN ""{1}"" {2} {3}",
                this.Name, field.Name, sqlType, notNull);
            db.Execute(sql);

            //添加注释
            sql = string.Format(
                "COMMENT ON COLUMN \"{0}\".\"{1}\" IS '{2}'",
                this.Name, field.Name, field.Label);
            db.Execute(sql);

            if (field.IsUnique)
            {
                this.AddUniqueConstraint(db, field.Name);
            }
        }

        public void DeleteColumn(IDBConnection db, string columnName)
        {
            if (db == null)
            {
                throw new ArgumentNullException("db");
            }
            if (string.IsNullOrEmpty(columnName))
            {
                throw new ArgumentNullException("columnName");
            }

            if (!NamingRule.IsValidSqlName(columnName))
            {
                throw new ArgumentOutOfRangeException("columnName");
            }

            var sql = string.Format(
                "ALTER TABLE \"{0}\" DROP COLUMN \"{1}\"",
                this.Name, columnName);
            db.Execute(sql);
        }

        public void AlterColumnNullable(IDBConnection db, string columnName, bool nullable)
        {
            if (db == null)
            {
                throw new ArgumentNullException("db");
            }

            if (string.IsNullOrEmpty(columnName))
            {
                throw new ArgumentNullException("columnName");
            }

            if (!NamingRule.IsValidSqlName(columnName))
            {
                throw new ArgumentOutOfRangeException("columnName");
            }

            var action = nullable ? "DROP NOT NULL" : "SET NOT NULL";
            var sql = string.Format(
                "ALTER TABLE \"{0}\" ALTER COLUMN \"{1}\" {2}",
                this.Name, columnName, action);
            db.Execute(sql);
        }

        public void AlterColumnType(IDBConnection db, string columnName, string sqlType)
        {
            if (db == null)
            {
                throw new ArgumentNullException("db");
            }

            if (string.IsNullOrEmpty(columnName))
            {
                throw new ArgumentNullException("columnName");
            }

            if (!NamingRule.IsValidSqlName(columnName))
            {
                throw new ArgumentOutOfRangeException("columnName");
            }

            var sql = string.Format(
                "ALTER TABLE \"{0}\" ALTER \"{1}\" TYPE {2}",
                this.Name, columnName, sqlType);
            db.Execute(sql);
        }

        public bool ColumnExists(string columnName)
        {
            if (string.IsNullOrEmpty(columnName))
            {
                throw new ArgumentNullException("columnName");
            }

            if (!NamingRule.IsValidSqlName(columnName))
            {
                throw new ArgumentOutOfRangeException("columnName");
            }

            return this.columns.ContainsKey(columnName);
        }

        public IColumnMetadata GetColumn(string columnName)
        {
            if (string.IsNullOrEmpty(columnName))
            {
                throw new ArgumentNullException("columnName");
            }

            if (!NamingRule.IsValidSqlName(columnName))
            {
                throw new ArgumentOutOfRangeException("columnName");
            }

            return this.columns[columnName];
        }

        public IColumnMetadata[] GetAllColumns()
        {
            Debug.Assert(this.columns != null);

            return this.columns.Values.ToArray();
        }

        private void LoadColumns(IDBConnection db, string tableName)
        {
            if (db == null)
            {
                throw new ArgumentNullException("db");
            }
            if (string.IsNullOrEmpty(tableName))
            {
                throw new ArgumentNullException("tableName");
            }
            if (!NamingRule.IsValidSqlName(tableName))
            {
                throw new ArgumentOutOfRangeException("tableName");
            }

            var sql = @"
SELECT column_name, data_type, is_nullable, character_maximum_length
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

        private void AddUniqueConstraint(IDBConnection db, string column)
        {
            Debug.Assert(db != null);
            Debug.Assert(!string.IsNullOrEmpty(column));

            var constraintName = this.Name + "_" + column + "_unique";
            var constraint = string.Format("UNIQUE(\"{0}\")", column);
            this.AddConstraint(db, constraintName, constraint);
        }


        public void AddConstraint(IDBConnection db, string constraintName, string constraint)
        {
            Debug.Assert(db != null);
            Debug.Assert(!string.IsNullOrEmpty(constraintName));
            Debug.Assert(!string.IsNullOrEmpty(constraint));

            var sql = "ALTER TABLE \"{0}\" ADD CONSTRAINT \"{1}\" {2}";
            sql = string.Format(sql, this.Name, constraintName, constraint);
            db.Execute(sql);
        }

        public void DeleteConstraint(IDBConnection db, string constraintName)
        {
            if (db == null)
            {
                throw new ArgumentNullException("db");
            }
            if (string.IsNullOrEmpty(constraintName))
            {
                throw new ArgumentNullException("constraintName");
            }

            var sql = string.Format(
                "ALTER TABLE \"{0}\" DROP CONSTRAINT \"{1}\"",
                this.Name, constraintName);
            db.Execute(sql);
        }


        public void AddFK(IDBConnection db, string columnName, string refTable, OnDeleteAction act)
        {
            if (db == null)
            {
                throw new ArgumentNullException("db");
            }
            if (string.IsNullOrEmpty(columnName))
            {
                throw new ArgumentNullException("columnName");
            }
            if (!NamingRule.IsValidSqlName(columnName))
            {
                throw new ArgumentOutOfRangeException("columnName");
            }
            if (string.IsNullOrEmpty(refTable))
            {
                throw new ArgumentNullException("refTable");
            }

            var onDelete = onDeleteMapping[act];
            var fkName = this.GenerateFkName(columnName);
            var sql = string.Format(
                "ALTER TABLE \"{0}\" ADD CONSTRAINT \"{1}\" FOREIGN KEY (\"{2}\") REFERENCES \"{3}\" ON DELETE {4}",
                this.Name, fkName, columnName, refTable, onDelete);

            db.Execute(sql);
        }


        public void DeleteFK(IDBConnection db, string columnName)
        {
            if (db == null)
            {
                throw new ArgumentNullException("db");
            }
            if (string.IsNullOrEmpty(columnName))
            {
                throw new ArgumentNullException("columnName");
            }
            if (!NamingRule.IsValidSqlName(columnName))
            {
                throw new ArgumentOutOfRangeException("columnName");
            }

            var fkName = this.GenerateFkName(columnName);
            this.DeleteConstraint(db, fkName);
        }

        public bool FKExists(IDBConnection db, string columnName)
        {
            if (db == null)
            {
                throw new ArgumentNullException("db");
            }
            if (string.IsNullOrEmpty(columnName))
            {
                throw new ArgumentNullException("columnName");
            }
            if (!NamingRule.IsValidSqlName(columnName))
            {
                throw new ArgumentOutOfRangeException("columnName");
            }

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
            if (string.IsNullOrEmpty(columnName))
            {
                throw new ArgumentNullException("columnName");
            }
            if (!NamingRule.IsValidSqlName(columnName))
            {
                throw new ArgumentOutOfRangeException("columnName");
            }

            return string.Format("{0}_{1}_fkey", this.Name, columnName);
        }
    }
}
