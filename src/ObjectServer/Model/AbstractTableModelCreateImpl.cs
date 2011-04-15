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
using ObjectServer.Core;

namespace ObjectServer.Model
{
    public abstract partial class AbstractTableModel : AbstractModel
    {
        public override long CreateInternal(IServiceScope scope, IDictionary<string, object> userRecord)
        {
            if (!this.CanCreate)
            {
                throw new NotSupportedException();
            }

            if (userRecord.ContainsKey("id"))
            {
                throw new ArgumentException("Unable to set the 'id' field", "propertyBag");
            }

            if (!ModelSecurity.CanCreateModel(scope, scope.Session.UserId, this.Name))
            {
                throw new UnauthorizedAccessException("Access denied");
            }

            var record = ClearUserRecord(userRecord);

            //处理用户没有给的默认值
            this.AddDefaultValues(scope, record);

            //插入被继承的表记录
            foreach (var i in this.Inheritances)
            {
                var baseModel = (IModel)scope.GetResource(i.BaseModel);
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

            this.VerifyFields(record.Keys);

            var selfId = this.CreateSelf(scope, record);
            this.PostcreateHierarchy(scope, selfId, record);
            this.PostcreateManyToManyFields(scope, selfId, record);

            if (this.LogCreation)
            {
                //TODO: 可翻译的
                this.AuditLog(scope, selfId, this.Label + " created");
            }

            return selfId;
        }

        /// <summary>
        /// 处理 Many-to-many 字段
        /// </summary>
        /// <param name="scope"></param>
        /// <param name="record"></param>
        /// <param name="id"></param>
        private void PostcreateManyToManyFields(
            IServiceScope scope, long id, Dictionary<string, object> record)
        {

            //处理 Many-to-many 字段
            var manyToManyFields =
                from fn in record.Keys
                let f = this.Fields[fn]
                where f.Type == FieldType.ManyToMany && !f.IsFunctional && !f.IsReadonly
                select f;

            foreach (var f in manyToManyFields)
            {
                var relModel = (IModel)scope.GetResource(f.Relation);
                //写入
                var targetIds = (long[])record[f.Name];

                foreach (var targetId in targetIds)
                {
                    var targetRecord = new Dictionary<string, object>(2);
                    targetRecord[f.OriginField] = id;
                    targetRecord[f.RelatedField] = targetId;
                    relModel.CreateInternal(scope, targetRecord);
                }
            }
        }

        private void PostcreateHierarchy(
            IServiceScope scope, long id, Dictionary<string, object> record)
        {
            //处理层次表
            if (this.Hierarchy)
            {
                long rhsValue = 0;
                //先检查是否给了 _parent 字段的值
                if (record.ContainsKey(ParentFieldName))
                {
                    var parentID = (long)record[ParentFieldName];
                    var sql = string.Format(
                        "SELECT _left, _right FROM \"{0}\" WHERE \"id\" = @0",
                        this.TableName);

                    var records = scope.Connection.QueryAsDictionary(sql, parentID);
                    if (records.Length == 0)
                    {
                        //TODO 使用合适的异常
                        throw new Exception("找不到记录");
                    }

                    //判断父节点是否是叶子节点
                    var left = (long)records[0][LeftFieldName];
                    var right = (long)records[0][RightFieldName];

                    if (right - left == 1)
                    {
                        rhsValue = left;
                    }
                    else
                    {
                        rhsValue = right;
                    }
                }
                else //没有就查找一个可用的
                {
                    var sql = string.Format("SELECT MAX(_right) FROM \"{0}\" ", this.TableName);
                    var value = scope.Connection.QueryValue(sql);
                    if (value != DBNull.Value)
                    {
                        rhsValue = (long)value;
                    }
                    else // 空表
                    {
                        rhsValue = 0;
                    }
                }

                //TODO: 需要锁表
                var sqlUpdate1 = string.Format(
                    "UPDATE \"{0}\" SET _right = _right + 2 WHERE _right > @0", this.TableName);
                var sqlUpdate2 = string.Format(
                    "UPDATE \"{0}\" SET _left = _left + 2 WHERE _left > @0", this.TableName);
                var sqlUpdate3 = string.Format(
                    "UPDATE \"{0}\" SET _left = @0, _right = @1 WHERE (\"id\" = @2) ", this.TableName);

                scope.Connection.Execute(sqlUpdate1, rhsValue);
                scope.Connection.Execute(sqlUpdate2, rhsValue);
                scope.Connection.Execute(sqlUpdate3, rhsValue + 1, rhsValue + 2, id);
            }
        }

        private long CreateSelf(IServiceScope ctx, IDictionary<string, object> values)
        {
            var serial = ctx.Connection.NextSerial(this.SequenceName);

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

            var rows = ctx.Connection.Execute(sql, colValues);
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
        private void AddDefaultValues(IServiceScope ctx, IDictionary<string, object> propertyBag)
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
