using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

            //创建字段
            if (this.model.Hierarchy)
            {
                //conn.ExecuteNonQuery("");
            }

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

        private void SyncTable(ITableContext table)
        {
            throw new NotImplementedException();
        }

    }
}
