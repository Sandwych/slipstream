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
        private IDatabase db;
        private IModel model;
        private IContext context;

        public TableMigrator(IDatabase db, IModel model)
        {
            this.db = db;
            this.model = model;
            this.context = new SessionlessContext(db);
        }

        public void Migrate()
        {
            Debug.Assert(this.db != null);
            Debug.Assert(this.model != null);

            var table = this.db.DataContext.CreateTableContext(this.model.TableName);

            if (!table.TableExists(db.DataContext, this.model.TableName))
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
            table.CreateTable(db.DataContext, this.model.TableName, this.model.Label);

            var storableColumns = this.model.GetAllStorableFields();

            foreach (var f in storableColumns)
            {
                table.AddColumn(db.DataContext, f);

                if (f.Type == FieldType.ManyToOne)
                {
                    var refModel = (TableModel)this.db.GetResource(f.Relation);
                    table.AddFK(db.DataContext, f.Name, refModel.TableName, ReferentialAction.SetNull);
                }
            }
        }

        /// <summary>
        /// 尝试同步表，有可能不成功
        /// </summary>
        /// <param name="table"></param>
        private void SyncTable(ITableContext table)
        {
            //表肯定存在，就看列存不存在
            //最简单的迁移策略：
            //如果列在表里不存在就建，以用户定义的为准
            //列元属性同步策略：
            //* 类型相同可以更改长度，其实也就是 varchar 可以
            //* 可空转非空：检测表里是否有行，有的话要求此字段有默认值，更改之前先用默认值填充
            //  现有行再转换为非空
            //* 非空转可控：可以直接去掉 NOT NULL
            foreach (var pair in this.model.Fields)
            {
                var field = pair.Value;
                if (field.IsColumn())
                {
                    if (!table.ColumnExists(field.Name))
                    {
                        table.AddColumn(this.db.DataContext, field);
                    }
                    else
                    {
                        this.SyncColumn(table, field);
                    }
                }
            }
        }

        private void SyncColumn(ITableContext table, IMetaField field)
        {
            //TODO 实现迁移策略
            var columnInfo = table.GetColumn(field.Name);

            //varchar(n) 类型的 n 变化了
            if (field.Type == FieldType.Chars && field.Size != columnInfo.Length)
            {
                //TODO:转换成可移植数据库类型    
                var sqlType = string.Format("VARCHAR({0})", field.Size);
                table.AlterColumnType(this.db.DataContext, field.Name, sqlType);
            }


            if (!columnInfo.Nullable && !field.Required) //"NOT NULL" to nullable
            {
                this.SetColumnNullable(table, field);
            }
            else if (columnInfo.Nullable && field.Required) //Nullable to 'NOT NULL'
            {
                this.SetColumnNotNullable(table, field);
            }
        }

        private void SetColumnNullable(ITableContext table, IMetaField field)
        {
            table.AlterColumnNullable(this.db.DataContext, field.Name, true);
        }

        private void SetColumnNotNullable(ITableContext table, IMetaField field)
        {
            //先看有没有行，有行要先设置默认值，如果没有默认值就报错了
            if (this.TableHasRow(table.Name) && field.DefaultProc != null)
            {
                var defaultValue = field.DefaultProc(this.context);
                var sql = string.Format(
                    "UPDATE \"{0}\" SET \"{1}\"=@0", table.Name, field.Name);
                this.db.DataContext.Execute(sql, defaultValue);
                table.AlterColumnNullable(this.db.DataContext, field.Name, false);
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
                "SELECT COUNT(\"id\") FROM \"{0}\"", tableName);
            var rowCount = (long)this.db.DataContext.QueryValue(sql);
            return rowCount > 0;
        }

    }
}
