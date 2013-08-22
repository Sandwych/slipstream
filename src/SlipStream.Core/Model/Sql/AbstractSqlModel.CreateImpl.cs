using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Data;
using System.Reflection;
using System.Dynamic;
using System.Globalization;

using Autofac;
using SlipStream.Exceptions;
using NHibernate.SqlCommand;
using Sandwych;

using SlipStream.Data;
using SlipStream.Core;

namespace SlipStream.Model
{
    using Record = Dictionary<string, object>;
    using IRecord = IDictionary<string, object>;

    public abstract partial class AbstractSqlModel : AbstractModel
    {
        public override long CreateInternal(IRecord userRecord)
        {
            if (!this.CanCreate)
            {
                throw new NotSupportedException();
            }

            var scope = this.DbDomain.CurrentSession;

            if (userRecord.ContainsKey(AbstractModel.IdFieldName))
            {
                var msg = string.Format("Unable to set the '{0}' field", AbstractModel.IdFieldName);
                throw new ArgumentException(msg, "propertyBag");
            }

            //检查是否有不存在的字段
            if (userRecord.Keys.Any(fn => !this.Fields.ContainsKey(fn)))
            {
                var msg = "Record contains one or more invalid field name";
                throw new ArgumentOutOfRangeException(msg);
            }

            var record =
                (from p in userRecord
                 let f = this.Fields[p.Key]
                 where (!SystemReadonlyFields.Contains(p.Key)) && (f.Type != FieldType.OneToMany)
                 select p
                ).ToDictionary(p => p.Key, p => p.Value);

            //处理用户没有给的默认值
            this.AddDefaultValuesForCreation(record);

            //校验用户提供的值是否满足字段约束
            this.ValidateRecordForCreation(record);

            //创建被继承表的记录
            this.PrecreateBaseRecords(record);

            //转换用户给的字段值到数据库原始类型
            this.ConvertFieldToColumn(record, record.Keys.ToArray());

            var selfId = this.CreateSelf(record);

            if (this.Hierarchy)
            {
                this.UpdateTreeForCreation(selfId, record);
            }

            this.UpdateOneToManyFields(selfId, record);

            this.PostcreateManyToManyFields(selfId, record);

            if (this.LogCreation)
            {
                //TODO: 可翻译的
                this.AuditLog(scope, selfId, this.Label + " created");
            }

            return selfId;
        }

        private void PrecreateBaseRecords(IRecord record)
        {
            foreach (var i in this.Inheritances)
            {
                var baseModel = (IModel)this.DbDomain.GetResource(i.BaseModel);
                var baseRecord = new Record();

                foreach (var f in baseModel.Fields)
                {
                    object fieldValue = null;
                    if (record.TryGetValue(f.Key, out fieldValue))
                    {
                        baseRecord.Add(f.Key, fieldValue);
                        record.Remove(f.Key);
                    }
                }
                var baseId = baseModel.CreateInternal(baseRecord);
                record[i.RelatedField] = baseId;
            }
        }

        /// <summary>
        /// 处理 Many-to-many 字段
        /// </summary>
        /// <param name="tc"></param>
        /// <param name="record"></param>
        /// <param name="id"></param>
        private void PostcreateManyToManyFields(long id, IRecord record)
        {

            //处理 Many-to-many 字段
            var manyToManyFields =
                from fn in record.Keys
                let f = this.Fields[fn]
                where f.Type == FieldType.ManyToMany && !f.IsFunctional && !f.IsReadonly
                select f;

            foreach (var f in manyToManyFields)
            {
                var relModel = (IModel)this.DbDomain.GetResource(f.Relation);
                //写入
                var targetIds = (long[])record[f.Name];

                foreach (var targetId in targetIds)
                {
                    var targetRecord = new Record(2);
                    targetRecord[f.OriginField] = id;
                    targetRecord[f.RelatedField] = targetId;
                    relModel.CreateInternal(targetRecord);
                }
            }
        }

        #region 处理层次表的创建事宜
        private void UpdateTreeForCreation(long id, Record record)
        {
            //如果支持存储过程就用存储/函数，那么直接调用预定义的存储过程或函数来处理
            var dataProvider = SlipstreamEnvironment.RootContainer.Resolve<IDataProvider>();
            if (dataProvider.IsSupportProcedure)
            {
                this.UpdateTreeForCreationBySqlFunction(id, record);
            }
            else
            {
                this.UpdateTreeForCreationBySqlStatements(id, record);
            }
        }

        /// <summary>
        /// 使用多条语句更新层次表，一般用于 SQLite 等不支持用户自定义存储过程或函数的数据库
        /// </summary>
        private void UpdateTreeForCreationBySqlStatements(long id, Record record)
        {
            var dbctx = this.DbDomain.CurrentSession.DataContext;
            //处理层次表
            long rhsValue = 0;
            //先检查是否给了 parent 字段的值
            object parentIDObj = 0;
            if (record.TryGetValue(ParentFieldName, out parentIDObj) && parentIDObj != null)
            {
                var parentID = (long)parentIDObj;
                var sql = new SqlString(
                    @" select ""_left"", ""_right"" from ", '"' + this.TableName + '"',
                    @" where ""_id""  = ", Parameter.Placeholder);

                var records = dbctx.QueryAsDictionary(sql, parentID);
                if (records.Length == 0)
                {
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
                    @" select max(""_right"")",
                    @" from ", '"' + this.TableName + '"',
                    @" where ", '"' + LeftFieldName + '"', ">=0");

                var value = dbctx.QueryValue(sql);
                if (!value.IsNull())
                {
                    rhsValue = (long)value;
                }
                else // 空表
                {
                    rhsValue = 0;
                }
            }

            //因为NestedSets 模型的关系，
            //我们修改的不止一条记录，所以这里需要锁定表，防止其它连接修改数据库
            dbctx.LockTable(this.TableName);

            var sqlUpdate1 = string.Format(CultureInfo.InvariantCulture,
                "update \"{0}\" set _right = _right + 2 where _right>?", this.TableName);
            dbctx.Execute(SqlString.Parse(sqlUpdate1), rhsValue);

            var sqlUpdate2 = string.Format(CultureInfo.InvariantCulture,
                "update \"{0}\" set _left = _left + 2 where _left>?", this.TableName);
            dbctx.Execute(SqlString.Parse(sqlUpdate2), rhsValue);

            var sqlUpdate3 = string.Format(CultureInfo.InvariantCulture,
                "update \"{0}\" set _left=?, _right=? where (_id=?) ", this.TableName);
            dbctx.Execute(SqlString.Parse(sqlUpdate3), rhsValue + 1, rhsValue + 2, id);
        }

        /// <summary>
        /// 如果数据库支持用户定义函数或存储过程的话就是用预定义函数或存储过程来更新表
        /// </summary>
        private void UpdateTreeForCreationBySqlFunction(long id, Record record)
        {
            var dbctx = this.DbDomain.CurrentSession.DataContext;
            using (var cmd = dbctx.CreateCommand(new SqlString("tree_update_for_creation")))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                var paramTableName = cmd.CreateParameter();
                paramTableName.ParameterName = "table_name";
                paramTableName.Value = this.TableName;
                cmd.Parameters.Add(paramTableName);

                var paramSelfId = cmd.CreateParameter();
                paramTableName.ParameterName = "self_id";
                paramSelfId.Value = id;
                cmd.Parameters.Add(paramSelfId);

                var paramParentId = cmd.CreateParameter();
                paramParentId.ParameterName = "parent_id";
                object parentId = null;
                if (record.TryGetValue(ParentFieldName, out parentId) && parentId != null)
                {
                    paramParentId.Value = parentId;
                }
                else
                {
                    paramParentId.Value = null;
                }
                cmd.Parameters.Add(paramParentId);

                cmd.ExecuteNonQuery();
            }
        }

        #endregion

        private long CreateSelf(IRecord values)
        {
            Debug.Assert(values != null);


            if (this.ContainsField(VersionFieldName) && !values.ContainsKey(VersionFieldName))
            {
                values.Add(VersionFieldName, 0);
            }

            var allColumnNames = from f in values.Keys
                                 let fieldInfo = this.Fields[f]
                                 where fieldInfo.IsColumn && fieldInfo.Name != IdFieldName
                                 select f;

            var colValues = new object[allColumnNames.Count()];

            var sql = this.BuildInsertStatement(values, allColumnNames, colValues);

            var rows = this.DbDomain.CurrentSession.DataContext.Execute(sql, colValues);
            if (rows != 1)
            {
                var msg = string.Format("Failed to insert row, SQL: {0}", sql);
                throw new SlipStream.Exceptions.DataException(msg);
            }

            return this.DbDomain.CurrentSession.DataContext.GetLastIdentity(this.TableName);
        }

        private SqlString BuildInsertStatement(IRecord values, IEnumerable<string> allColumnNames, object[] colValues)
        {
            // "insert into <tableName> (_id, cols... ) values (<id>, ?, ?, ?... );",
            var sqlBuilder = new SqlStringBuilder();
            sqlBuilder.Add("insert into ");
            sqlBuilder.Add(quotedTableName);
            sqlBuilder.Add("(");

            var index = 0;
            foreach (var f in allColumnNames)
            {
                colValues[index] = values[f];
                if (index != 0)
                {
                    sqlBuilder.Add(",");
                }
                sqlBuilder.Add('"' + f + '"');
                index++;
            }

            sqlBuilder.Add(") values (");

            for (int i = 0; i < allColumnNames.Count(); i++)
            {
                if (i != 0)
                {
                    sqlBuilder.Add(",");
                }
                sqlBuilder.Add(Parameter.Placeholder);
            }

            sqlBuilder.Add(")");
            return sqlBuilder.ToSqlString();
        }

        /// <summary>
        /// 添加没有包含在字典 dict 里但是有默认值函数的字段
        /// </summary>
        /// <param name="session"></param>
        /// <param name="values"></param>
        private void AddDefaultValuesForCreation(IRecord propertyBag)
        {
            var ctx = this.DbDomain.CurrentSession;

            var defaultFields =
                this.Fields.Values.Where(
                d => (d.DefaultProc != null && !propertyBag.Keys.Contains(d.Name)));

            foreach (var df in defaultFields)
            {
                propertyBag[df.Name] = df.DefaultProc(ctx);
            }

            if (this.Fields.ContainsKey(UpdatedTimeFieldName))
            {
                if (this.Fields.ContainsKey(CreatedTimeFieldName))
                {
                    propertyBag[UpdatedTimeFieldName] = propertyBag[CreatedTimeFieldName];
                }
                else
                {
                    propertyBag[UpdatedTimeFieldName] = DateTime.Now;
                }
            }

            if (this.Fields.ContainsKey(UpdatedUserFieldName))
            {
                if (this.Fields.ContainsKey(CreatedUserFieldName))
                {
                    propertyBag[UpdatedUserFieldName] = propertyBag[CreatedUserFieldName];
                }
                else if (!ctx.UserSession.IsSystemUser)
                {
                    propertyBag[UpdatedUserFieldName] = ctx.UserSession.UserId;
                }
            }
        }
    }
}
