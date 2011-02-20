using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ObjectServer.Backend;

namespace ObjectServer.Model
{
    internal class TableMigrator
    {
        private IDatabase db;
        private IModel model;
        private ObjectPool pool;

        public TableMigrator(IDatabase db, ObjectPool pool, IModel model)
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

        private void CreateTable(ITable table)
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
                    var refModel = (TableModel)this.pool.LookupObject(f.Relation);
                    table.AddFk(db, f.Name, refModel.TableName, ReferentialAction.SetNull);
                }
            }
        }

        private void SyncTable(ITable table)
        {
            throw new NotImplementedException();
        }

    }
}
