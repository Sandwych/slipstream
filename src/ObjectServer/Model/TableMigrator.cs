using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using ObjectServer.Backend;

namespace ObjectServer.Model
{
    internal class TableMigrator
    {
        private IDBProfile db;
        private IModel model;
        private IServiceScope context;

        public TableMigrator(IDBProfile db, IModel model)
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
            this.context = new SessionlessServiceScope(db);
        }

        public void Migrate()
        {
            Debug.Assert(this.db != null);

            var table = this.db.Connection.CreateTableContext(this.model.TableName);

            if (!table.TableExists(db.Connection, this.model.TableName))
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
            table.CreateTable(db.Connection, this.model.TableName, this.model.Label);

            var storableColumns = this.model.GetAllStorableFields();

            foreach (var f in storableColumns)
            {
                table.AddColumn(db.Connection, f);

                if (f.Type == FieldType.ManyToOne)
                {
                    var resources = this.db as IResourceContainer; //dyanmic workaround
                    var refModel = (IModel)resources.GetResource(f.Relation);
                    table.AddFK(db.Connection, f.Name, refModel.TableName, OnDeleteAction.SetNull);
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
                        table.AddColumn(this.db.Connection, field);
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
                        table.AlterColumnNullable(this.db.Connection, c.Name, true);
                    }
                }
            }
            else
            {
                foreach (var c in columnsToDelete)
                {
                    table.DeleteColumn(this.db.Connection, c);
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
                table.AlterColumnType(this.db.Connection, field.Name, sqlType);
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
            table.AlterColumnNullable(this.db.Connection, field.Name, true);
        }

        private void SetColumnNotNullable(ITableContext table, IField field, bool hasRow)
        {
            //先看有没有行，有行要先设置默认值，如果没有默认值就报错了
            if (hasRow && field.DefaultProc != null)
            {
                var defaultValue = field.DefaultProc(this.context);
                var sql = string.Format(
                    "UPDATE \"{0}\" SET \"{1}\"=@0", table.Name, field.Name);
                this.db.Connection.Execute(sql, defaultValue);
                table.AlterColumnNullable(this.db.Connection, field.Name, false);
            }
            else
            {
                Logger.Warn(() => string.Format(
                    "unable alter table '{0}' column '{1}' to not nullable", table.Name, field.Name));
            }
        }

        private bool TableHasRow(string tableName)
        {
            var sql = string.Format(
                "SELECT COUNT(*) FROM \"{0}\"", tableName);
            var rowCount = (long)this.db.Connection.QueryValue(sql);
            return rowCount > 0;
        }

    }
}
