using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Reflection;
using System.Diagnostics;

using NHibernate.SqlCommand;

using ObjectServer.Model;
using ObjectServer.Utility;

namespace ObjectServer.Data.Postgresql
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

        public PgTableContext(IDBContext db, string tableName)
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

        public bool TableExists(IDBContext db, string tableName)
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
            var sql = new SqlString(
                "select coalesce(count(table_name), 0) ",
                "from information_schema.tables ",
                "where table_type='BASE TABLE' and table_schema='public' and table_name=",
                Parameter.Placeholder);

            var n = (long)db.QueryValue(sql, tableName);
            return n > 0;
        }

        public void CreateTable(IDBContext db, IModelDescriptor model, string label)
        {
            if (db == null)
            {
                throw new ArgumentNullException("db");
            }

            if (model == null)
            {
                throw new ArgumentNullException("model");
            }

            var tableName = model.TableName.SqlEscape();

            LoggerProvider.PlatformLogger.Info(String.Format("Creating Table [{0}]...", tableName));

            var fieldsWithoutId = model.Fields.Values.Where(f => f.IsColumn());

            var sb = new SqlStringBuilder();
            sb.Add("create table ");
            sb.Add(DataProvider.Dialect.QuoteForTableName(tableName));
            sb.Add("(");

            var commaNeeded = false;
            foreach (var f in fieldsWithoutId)
            {
                if (commaNeeded)
                {
                    sb.Add(", ");
                }
                commaNeeded = true;

                sb.Add(DataProvider.Dialect.QuoteForColumnName(f.Name));
                sb.Add(" ");
                var sqlType = PgSqlTypeConverter.GetSqlType(f);
                sb.Add(sqlType);
            }

            sb.Add(") without oids");

            var sql = sb.ToSqlString();
            db.Execute(sql);

            SetTableComment(db, tableName, label);
        }

        public void CreateTable(IDBContext db, string tableName, string label)
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

            LoggerProvider.PlatformLogger.Info(String.Format("Creating Table [{0}]...", tableName));

            tableName = tableName.SqlEscape();
            var sb = new SqlStringBuilder();
            sb.Add("create table ");
            sb.Add(DataProvider.Dialect.QuoteForTableName(tableName));
            sb.Add("(");
            sb.Add("_id");
            sb.Add(" ");
            sb.Add("bigserial not null, ");
            sb.Add("primary key(");
            sb.Add("_id");
            sb.Add(")) without oids");

            var sql = sb.ToSqlString();
            db.Execute(sql);

            SetTableComment(db, tableName, label);
        }

        private static void SetTableComment(IDBContext db, string tableName, string label)
        {
            if (!String.IsNullOrEmpty(label))
            {
                label = label.SqlEscape();
                var sql = new SqlString("comment on table ",
                    DataProvider.Dialect.QuoteForTableName(tableName),
                    " is '", label.SqlEscape(), "'");

                db.Execute(sql);
            }
        }

        public void AddColumn(IDBContext db, IFieldDescriptor field)
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
            var sql = new SqlString("alter table ",
                DataProvider.Dialect.QuoteForTableName(this.Name),
                " add column ",
                DataProvider.Dialect.QuoteForColumnName(field.Name), " ", sqlType);

            db.Execute(sql);

            this.SetColumnComment(db, field);

            if (field.IsUnique)
            {
                this.AddUniqueConstraint(db, field.Name);
            }
        }

        private void SetColumnComment(IDBContext db, IFieldDescriptor field)
        {
            if (!string.IsNullOrEmpty(field.Label))
            {
                //添加注释
                var commentSql = new SqlString(
                     "comment on column ",
                     DataProvider.Dialect.QuoteForTableName(this.Name),
                     ".",
                     DataProvider.Dialect.QuoteForColumnName(field.Name),
                     " is '", field.Label.SqlEscape(), "'");
                db.Execute(commentSql);
            }
        }

        public void DeleteColumn(IDBContext db, string columnName)
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
                "alter table \"{0}\" drop column \"{1}\"",
                this.Name, columnName);
            db.Execute(SqlString.Parse(sql));
        }

        public void AlterColumnNullable(IDBContext db, string columnName, bool nullable)
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

            var action = nullable ? "drop not null" : "set not null";
            var sql = string.Format(
                "alter table \"{0}\" alter column \"{1}\" {2}",
                this.Name, columnName, action);
            db.Execute(SqlString.Parse(sql));
        }

        public void AlterColumnType(IDBContext db, string columnName, string sqlType)
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
                "alter table \"{0}\" alter \"{1}\" type {2}",
                this.Name, columnName, sqlType);
            db.Execute(SqlString.Parse(sql));
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

        private void LoadColumns(IDBContext db, string tableName)
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

            var sql = new SqlString(
            "select column_name, data_type, is_nullable, character_maximum_length ",
            "from information_schema.columns ",
            "where table_name=", Parameter.Placeholder, " ",
            "order by ordinal_position");

            var records = db.QueryAsDictionary(sql, tableName);
            this.columns.Clear();
            foreach (var r in records)
            {
                var column = new PgColumnMetadata(r);
                this.columns.Add(column.Name, column);
            }

        }

        private void AddUniqueConstraint(IDBContext db, string column)
        {
            Debug.Assert(db != null);
            Debug.Assert(!string.IsNullOrEmpty(column));

            var constraintName = this.Name + "_" + column + "_unique";
            var constraint = string.Format("UNIQUE(\"{0}\")", column);
            this.AddConstraint(db, constraintName, constraint);
        }


        public void AddConstraint(IDBContext db, string constraintName, string constraint)
        {
            Debug.Assert(db != null);
            Debug.Assert(!string.IsNullOrEmpty(constraintName));
            Debug.Assert(!string.IsNullOrEmpty(constraint));

            var sql = "alter table \"{0}\" add constraint \"{1}\" {2}";
            sql = string.Format(sql, this.Name, constraintName, constraint);
            db.Execute(SqlString.Parse(sql));
        }

        public void DeleteConstraint(IDBContext db, string constraintName)
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
                "alter table \"{0}\" drop constraint \"{1}\"",
                this.Name, constraintName);
            db.Execute(SqlString.Parse(sql));
        }


        public bool ConstraintExists(IDBContext db, string constraintName)
        {
            //TODO ESCAPE SQL
            if (db == null)
            {
                throw new ArgumentNullException("db");
            }
            if (string.IsNullOrEmpty(constraintName))
            {
                throw new ArgumentNullException("constraintName");
            }

            var sql = SqlString.Parse(@"
select coalesce(count(constraint_name), 0)
    from information_schema.table_constraints
    where table_catalog=? and constraint_schema = 'public' and constraint_name=?");

            var n = (long)db.QueryValue(sql, db.DatabaseName, constraintName);
            return n > 0;
        }

        public void AddFK(IDBContext db, string columnName, string refTable, OnDeleteAction act)
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
                "alter table \"{0}\" add constraint \"{1}\" foreign key (\"{2}\") references \"{3}\" on delete {4}",
                this.Name, fkName, columnName, refTable, onDelete);

            db.Execute(SqlString.Parse(sql));
        }


        public void DeleteFK(IDBContext db, string columnName)
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

        public bool FKExists(IDBContext db, string columnName)
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

            var sql = SqlString.Parse(@"
select coalesce(count(constraint_name), 0)
    from information_schema.key_column_usage 
    where constraint_schema='public' and table_name=? and column_name=?
");
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
