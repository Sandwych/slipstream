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
        private IDatabaseContext db;
        private IModel model;
        private ObjectPool pool;

        public TableMigrator(IDatabaseContext db, ObjectPool pool, IModel model)
        {
            this.db = db;
            this.model = model;
            this.pool = pool;
        }

        public void Migrate()
        {
            Debug.Assert(this.db != null);
            Debug.Assert(this.model != null);
            Debug.Assert(this.pool != null);

            var table = this.db.CreateTableHandler(db, this.model.TableName);

            if (!table.TableExists(db, this.model.TableName))
            {
                this.CreateTable(table);
            }
            else
            {
                //this.SyncTable();
            }
        }

        private void CreateTable(ITableContext table)
        {
            table.CreateTable(db, this.model.TableName, this.model.Label);

            var storableColumns = this.model.GetAllStorableFields();

            foreach (var f in storableColumns)
            {
                table.AddColumn(db, f);

                if (f.Type == FieldType.ManyToOne)
                {
                    var refModel = (TableModel)this.pool[f.Relation];
                    table.AddFK(db, f.Name, refModel.TableName, ReferentialAction.SetNull);
                }
            }
        }

        private void SyncTable(IDatabaseContext db, ITableContext table)
        {
            //表肯定存在，就看列存不存在
            //最简单的迁移策略：
            //如果列在表里不存在就建，以用户定义的为主
            foreach (var pair in this.model.DefinedFields)
            {
                var field = pair.Value;
                if (field.IsStorable() && !table.ColumnExists(field.Name))
                {
                    table.AddColumn(db, field);
                }
            }
        }

    }
}
