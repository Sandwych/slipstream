using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Data;
using System.Reflection;
using System.Dynamic;

using ObjectServer.Backend;
using ObjectServer.Utility;
using ObjectServer.SqlTree;

namespace ObjectServer.Model
{
    public abstract partial class AbstractTableModel : AbstractModel
    {
        public override long CreateInternal(IResourceScope scope, IDictionary<string, object> userRecord)
        {
            if (!this.CanCreate)
            {
                throw new NotSupportedException();
            }

            if (userRecord.ContainsKey("id"))
            {
                throw new ArgumentException("Unable to set the 'id' field", "propertyBag");
            }

            var record = ClearUserRecord(userRecord);

            //处理用户没有给的默认值
            this.AddDefaultValues(scope, record);

            //插入被继承的表记录
            foreach (var i in this.Inheritances)
            {
                var baseModel = (IMetaModel)scope.GetResource(i.BaseModel);
                var baseRecord = new Dictionary<string, object>();

                foreach (var f in baseModel.Fields)
                {
                    if (record.ContainsKey(f.Key))
                    {
                        baseRecord.Add(f.Key, record[f.Key]);
                        record.Remove(f.Key);
                    }
                }

                var baseId = baseModel.CreateInternal(scope, baseRecord);
                record[i.RelatedField] = baseId;
            }


            //转换用户给的字段值到数据库原始类型
            this.ConvertFieldToColumn(scope, record, record.Keys.ToArray());

            var id = DoCreate(scope, record);

            if (this.LogCreation)
            {
                //TODO: 可翻译的
                this.AuditLog(scope, id, this.Label + " created");
            }

            return id;
        }

        private long DoCreate(IResourceScope ctx, IDictionary<string, object> values)
        {
            this.VerifyFields(values.Keys);

            var serial = ctx.DatabaseProfile.DataContext.NextSerial(this.SequenceName);

            if (this.ContainsField(VersionFieldName))
            {
                values.Add(VersionFieldName, 0);
            }

            var allColumnNames = values.Keys.Where(f => this.Fields[f].IsColumn());

            var colValues = new object[allColumnNames.Count()];
            var sbColumns = new StringBuilder();
            var sbArgs = new StringBuilder();
            var index = 0;
            foreach (var f in allColumnNames)
            {
                sbColumns.Append(", ");
                sbArgs.Append(", ");

                colValues[index] = values[f];

                sbArgs.Append("@" + index.ToString());
                sbColumns.Append('\"');
                sbColumns.Append(f);
                sbColumns.Append('\"');
                index++;
            }

            var columnNames = sbColumns.ToString();
            var args = sbArgs.ToString();

            var sql = string.Format(
              "INSERT INTO \"{0}\" (\"id\" {1}) VALUES ( {2} {3} );",
              this.TableName,
              columnNames,
              serial,
              args);

            var rows = ctx.DatabaseProfile.DataContext.Execute(sql, colValues);
            if (rows != 1)
            {
                Logger.Error(() => string.Format("Failed to insert row, SQL: {0}", sql));
                throw new DataException();
            }


            return serial;
        }

        /// <summary>
        /// 添加没有包含在字典 dict 里但是有默认值函数的字段
        /// </summary>
        /// <param name="session"></param>
        /// <param name="values"></param>
        private void AddDefaultValues(IResourceScope ctx, IDictionary<string, object> propertyBag)
        {
            var defaultFields =
                this.Fields.Values.Where(
                d => (d.DefaultProc != null && !propertyBag.Keys.Contains(d.Name)));

            foreach (var df in defaultFields)
            {
                propertyBag[df.Name] = df.DefaultProc(ctx);
            }
        }
    }
}
