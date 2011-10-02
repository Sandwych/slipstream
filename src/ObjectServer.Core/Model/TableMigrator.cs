using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using NHibernate.SqlCommand;

using ObjectServer.Data;

namespace ObjectServer.Model
{
    internal class TableMigrator : IDisposable
    {
        private IDBContext db;
        private AbstractTableModel model;
        private IServiceContext context;
        private bool disposed = false;

        public TableMigrator(IDBContext db, AbstractTableModel model)
        {
            if (db == null)
            {
                throw new ArgumentNullException("db");
            }

            if (model == null)
            {
                throw new ArgumentNullException("model");
            }

            this.db = db;
            this.model = model;
            this.context = new SystemTransactionContext(db);
        }

        public void Migrate()
        {
            Debug.Assert(this.db != null);

            var table = this.db.CreateTableContext(this.model.TableName);

            if (!table.TableExists(db, this.model.TableName))
            {
                this.CreateTable(table);
            }
            else
            {
                this.SyncTable(table);
            }
        }

        private void CreateTable(ITableContext table)
        {
            table.CreateTable(db, this.model, this.model.Name);

            CreateForeignKeys(table, this.model.Fields.Values);

            //为 _left 和 _right 添加索引
            if (this.model.Hierarchy)
            {
                //TODO 添加索引
            }
        }

        private void CreateForeignKeys(ITableContext table, IEnumerable<IField> fields)
        {
            var resources = Environment.DBProfiles.GetDBProfile(this.db.DatabaseName);
            foreach (var f in fields)
            {
                if (f.IsColumn() && f.Type == FieldType.ManyToOne)
                {
                    var refModel = (IModel)resources.GetResource(f.Relation);
                    table.AddFK(db, f.Name, refModel.TableName, OnDeleteAction.SetNull);
                }
            }
        }

        /// <summary>
        /// 尝试同步表，有可能不成功
        /// </summary>
        /// <param name="table"></param>
        private void SyncTable(ITableContext table)
        {
            bool hasRow = this.TableHasRow(table.Name);

            //表肯定存在，就看列存不存在
            //最简单的迁移策略：
            //如果列在表里不存在就建，以用户定义的为准
            //列元属性同步策略：
            //* 类型相同可以更改长度，其实也就是 varchar 可以
            //* 可空转非空：检测表里是否有行，有的话要求此字段有默认值，更改之前先用默认值填充
            //  现有行再转换为非空
            //* 非空转可控：可以直接去掉 NOT NULL
            //先处理代码里定义的列
            foreach (var pair in this.model.Fields)
            {
                var field = pair.Value;
                if (field.IsColumn())
                {
                    if (!table.ColumnExists(field.Name))
                    {
                        table.AddColumn(this.db, field);
                    }
                    else
                    {
                        this.SyncColumn(table, field, hasRow);
                    }
                }
            }

            //然后处理数据库里存在，但代码里未定义的列
            //处理策略很简单，我们的原则是不能删除用户的数据，如果是空表直接删除该列，如果有数据就把该列设成可空
            var columns = table.GetAllColumns();
            var columnsToDelete = columns.Select(c => c.Name)
                .Except(this.model.Fields.Select(f => f.Value.Name));
            //删除数据库存在，但代码未定义的

            if (hasRow)
            {
                foreach (var c in columns)
                {
                    if (!c.Nullable && columnsToDelete.Contains(c.Name))
                    {
                        table.AlterColumnNullable(this.db, c.Name, true);
                    }
                }
            }
            else
            {
                foreach (var c in columnsToDelete)
                {
                    table.DeleteColumn(this.db, c);
                }
            }

        }

        private void SyncColumn(ITableContext table, IField field, bool hasRow)
        {
            //TODO 实现迁移策略
            var columnInfo = table.GetColumn(field.Name);

            //varchar(n) 类型的 n 变化了
            if (field.Type == FieldType.Chars && field.Size != columnInfo.Length)
            {
                //TODO:转换成可移植数据库类型    
                var sqlType = string.Format("VARCHAR({0})", field.Size);
                table.AlterColumnType(this.db, field.Name, sqlType);
            }


            if (!columnInfo.Nullable && !field.IsRequired) //"NOT NULL" to nullable
            {
                this.SetColumnNullable(table, field);
            }
            else if (columnInfo.Nullable && field.IsRequired) //Nullable to 'NOT NULL'
            {
                this.SetColumnNotNullable(table, field, hasRow);
            }
        }

        private void SetColumnNullable(ITableContext table, IField field)
        {
            table.AlterColumnNullable(this.db, field.Name, true);
        }

        private void SetColumnNotNullable(ITableContext table, IField field, bool hasRow)
        {
            var dialect = DataProvider.Dialect;
            //先看有没有行，有行要先设置默认值，如果没有默认值就报错了
            if (hasRow && field.DefaultProc != null)
            {
                var defaultValue = field.DefaultProc(this.context);
                var sql = new SqlString("update ",
                    dialect.QuoteForTableName(table.Name),
                    " set ",
                    dialect.QuoteForColumnName(field.Name), "=", Parameter.Placeholder);
                this.db.Execute(sql, defaultValue);
                table.AlterColumnNullable(this.db, field.Name, false);
            }
            else
            {
                LoggerProvider.EnvironmentLogger.Warn(() => string.Format(
                    "unable alter table '{0}' column '{1}' to not nullable", table.Name, field.Name));
            }
        }

        private bool TableHasRow(string tableName)
        {
            var sql = new SqlString("select count(*) from ",
                DataProvider.Dialect.QuoteForTableName(tableName));
            var rowCount = (long)this.db.QueryValue(sql);
            return rowCount > 0;
        }


        private void Dispose(bool isDisposing)
        {
            if (!this.disposed)
            {
                if (isDisposing)
                {
                    //处置托管对象
                }

                this.context.Dispose();
                this.disposed = true;
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
