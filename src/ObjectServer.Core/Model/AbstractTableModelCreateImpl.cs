using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Data;
using System.Reflection;
using System.Dynamic;

using ObjectServer.Exceptions;
using NHibernate.SqlCommand;

using ObjectServer.Data;
using ObjectServer.Utility;
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

            if (userRecord.ContainsKey(AbstractModel.IDFieldName))
            {
                var msg = string.Format("Unable to set the '{0}' field", AbstractModel.IDFieldName);
                throw new ArgumentException(msg, "propertyBag");
            }

            if (!scope.CanCreateModel(scope.Session.UserId, this.Name))
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

            if (this.Hierarchy)
            {
                this.PostcreateHierarchy(scope.DBContext, selfId, record);
            }

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
            IDBContext conn, long id, Dictionary<string, object> record)
        {
            //处理层次表
            long rhsValue = 0;
            //先检查是否给了 _parent 字段的值
            if (record.ContainsKey(ParentFieldName))
            {
                var parentID = (long)record[ParentFieldName];
                var sql = new SqlString(
                    "select ",
                    DataProvider.Dialect.QuoteForColumnName(AbstractTableModel.LeftFieldName),
                    ",",
                    DataProvider.Dialect.QuoteForColumnName(AbstractTableModel.RightFieldName),
                    " from ",
                    DataProvider.Dialect.QuoteForTableName(this.TableName),
                    " where ",
                    DataProvider.Dialect.QuoteForColumnName(AbstractModel.IDFieldName),
                    "=",
                    Parameter.Placeholder);

                var records = conn.QueryAsDictionary(sql, parentID);
                if (records.Length == 0)
                {
                    //TODO 使用合适的异常
                    throw new RecordNotFoundException("Cannot found hierarchy record(s)", this.Name);
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
                    rhsValue = right - 1; //添加到集合的末尾
                }
            }
            else //没有就查找一个可用的
            {
                //"SELECT MAX(_right) FROM <TableName> WHERE _left >= 0"
                var sql = new SqlString(
                    "select max(",
                    DataProvider.Dialect.QuoteForColumnName(RightFieldName),
                    ") from ",
                    DataProvider.Dialect.QuoteForTableName(this.TableName),
                    " where ",
                    DataProvider.Dialect.QuoteForColumnName(LeftFieldName),
                    ">=0");

                var value = conn.QueryValue(sql);
                if (!value.IsNull())
                {
                    rhsValue = (long)value;
                }
                else // 空表
                {
                    rhsValue = 0;
                }
            }

            var sqlUpdate1 = string.Format(
                "update \"{0}\" set _right = _right + 2 where _right>?", this.TableName);
            var sqlUpdate2 = string.Format(
                "update \"{0}\" set _left = _left + 2 where _left>?", this.TableName);
            var sqlUpdate3 = string.Format(
                "update \"{0}\" set _left=?, _right=? where (_id=?) ", this.TableName);

            //conn.LockTable(this.TableName); //TODO 这里需要锁定表，防止其它连接修改
            conn.Execute(SqlString.Parse(sqlUpdate1), rhsValue);
            conn.Execute(SqlString.Parse(sqlUpdate2), rhsValue);
            conn.Execute(SqlString.Parse(sqlUpdate3), rhsValue + 1, rhsValue + 2, id);
        }

        private long CreateSelf(IServiceScope ctx, IDictionary<string, object> values)
        {
            var serial = ctx.DBContext.NextSerial(this.SequenceName);

            if (this.ContainsField(VersionFieldName))
            {
                values.Add(VersionFieldName, 0);
            }

            var allColumnNames = values.Keys.Where(f => this.Fields[f].IsColumn());
            var colValues = new object[allColumnNames.Count()];

            // "insert into <tableName> (_id, cols... ) values (<id>, ?, ?, ?... );",
            var sqlBuilder = new SqlStringBuilder();
            sqlBuilder.Add("insert into ");
            sqlBuilder.Add(quotedTableName);
            sqlBuilder.Add("(");
            sqlBuilder.Add(QuotedIdColumn);

            var index = 0;
            foreach (var f in allColumnNames)
            {
                colValues[index] = values[f];
                sqlBuilder.Add(",");
                sqlBuilder.Add(DataProvider.Dialect.QuoteForColumnName(f));
                index++;
            }

            sqlBuilder.Add(") values (");
            sqlBuilder.Add(serial.ToString());

            for (int i = 0; i < allColumnNames.Count(); i++)
            {
                sqlBuilder.Add(",");
                sqlBuilder.Add(Parameter.Placeholder);
            }

            sqlBuilder.Add(")");

            var sql = sqlBuilder.ToSqlString();

            var rows = ctx.DBContext.Execute(sql, colValues);
            if (rows != 1)
            {
                var msg = string.Format("Failed to insert row, SQL: {0}", sql);
                throw new ObjectServer.Exceptions.DataException(msg);
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
