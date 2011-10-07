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

using NHibernate.SqlCommand;

using ObjectServer.Exceptions;
using ObjectServer.Data;
using ObjectServer.Utility;

namespace ObjectServer.Model
{
    using Record = Dictionary<string, object>;
    using IRecord = IDictionary<string, object>;

    public abstract partial class AbstractSqlModel : AbstractModel
    {
        private static readonly string[] SearchParentNodeFields = new string[] { IdFieldName, LeftFieldName, RightFieldName };

        public override void WriteInternal(ITransactionContext ctx, long id, IRecord userRecord)
        {
            if (ctx == null)
            {
                throw new ArgumentNullException("ctx");
            }

            if (userRecord == null || userRecord.Count == 0)
            {
                throw new ArgumentNullException("userRecord");
            }

            if (!this.CanWrite)
            {
                throw new NotSupportedException();
            }

            if (!ctx.CanWriteModel(ctx.Session.UserId, this.Name))
            {
                throw new SecurityException("Access denied");
            }

            var record = ClearUserRecord(userRecord);
            ModelValidator.ValidateRecordForWriting(this, record);

            var isParentChanged = false;
            long? oldParentID = null;
            IRecord existedRecord = null;


            //处理版本字段与基类继承
            if (userRecord.ContainsKey(VersionFieldName) || this.Inheritances.Count > 0 || this.Hierarchy)
            {
                var fieldsToRead = new List<string>();

                //如果包含版本字段，那么我们需要先读取版本字段
                //TODO 在继承树上查找
                if (this.Fields.ContainsKey(VersionFieldName))
                {
                    fieldsToRead.Add(VersionFieldName);
                }

                //如果此表使用了继承，那么我们还需要读取关联到父表的字段
                if (this.Inheritances.Count > 0)
                {
                    foreach (var i in this.Inheritances)
                    {
                        Debug.Assert(!string.IsNullOrEmpty(i.RelatedField));
                        fieldsToRead.Add(i.RelatedField);
                    }
                }

                if (this.Hierarchy)
                {
                    fieldsToRead.Add(LeftFieldName);
                    fieldsToRead.Add(RightFieldName);

                    if (this.Fields.ContainsKey(ParentFieldName))
                    {
                        fieldsToRead.Add(ParentFieldName);
                    }
                }

                existedRecord = this.ReadInternal(ctx, new long[] { id }, fieldsToRead.ToArray()).First();

                this.VerifyRecordVersion(id, userRecord, existedRecord);

                this.PrewriteBaseModels(ctx, record, existedRecord);

                if (userRecord.ContainsKey(ParentFieldName))
                {
                    var newParentId = userRecord[ParentFieldName] as long?;
                    var oldParentValue = existedRecord[ParentFieldName] as object[];
                    if (oldParentValue != null)
                    {
                        oldParentID = (long)oldParentValue.First();
                    }
                    if ((newParentId.HasValue && (oldParentID == null || oldParentID.Value != newParentId.Value))
                        || (!newParentId.HasValue && oldParentID.HasValue))
                    {
                        isParentChanged = true;
                    }
                }
            }

            var rowsAffected = this.WriteSelf(ctx, id, record);

            //检查更新结果
            if (rowsAffected != 1)
            {
                var msg = string.Format("不能更新 ['{0}', {1}]，因为其已经被其它用户更新",
                    this.TableName, id);
                throw new ConcurrencyException(msg);
            }

            //更新层次结构
            //算法来自于：
            //http://stackoverflow.com/questions/889527/mysql-move-node-in-nested-set
            // 1.   Change positions of node ant all it's sub nodes into negative values, 
            //      which are equal to current ones by module.
            // 2.   Move all positions "up", which are more, that pos_right of current node.
            // 3.   Move all positions "down", which are more, that pos_right of new parent node.
            // 4.   Change positions of current node and all it's subnodes, 
            //      so that it's now will be exactly "after" (or "down") of new parent node.
            if (isParentChanged && Hierarchy)
            {
                var nodeLeft = (long)existedRecord[LeftFieldName];
                var nodeRight = (long)existedRecord[RightFieldName];
                var nodeWidth = nodeRight - nodeLeft + 1;

                long newParentID = 0;
                object newParentIDObj = null;
                if (userRecord.TryGetValue(ParentFieldName, out newParentIDObj) && newParentIDObj != null)
                {
                    newParentID = (long)newParentIDObj;
                }

                ctx.DBContext.LockTable(this.TableName); //层次表移动节点的时候需要锁表，因为涉及不止一行
                this.NodeMoveTo(ctx, nodeLeft, nodeRight, nodeWidth, newParentID);
            }

            if (this.LogWriting)
            {
                AuditLog(ctx, (long)id, this.Label + " updated");
            }
        }

        private int WriteSelf(ITransactionContext ctx, long id, Record record)
        {
            var allFields = record.Keys; //记录中的所有字段

            //先写入 many-to-many 字段
            this.PrewriteManyToManyFields(ctx, id, record, allFields);

            this.SetModificationInfo(ctx, record);

            //所有可更新的字段
            var updatableColumnFields = allFields.Where(
                f => this.Fields[f].IsColumn && !this.Fields[f].IsReadonly).ToArray();
            this.ConvertFieldToColumn(ctx, record, updatableColumnFields);

            var sqlBuilder = new SqlStringBuilder();
            sqlBuilder.Add("update ");
            sqlBuilder.Add(DataProvider.Dialect.QuoteForTableName(this.TableName));
            sqlBuilder.Add(" set ");

            var args = new object[updatableColumnFields.Count()];
            var argIndex = 0;
            var commaNeeded = false;
            foreach (var field in updatableColumnFields)
            {
                if (commaNeeded)
                {
                    sqlBuilder.Add(",");
                }
                commaNeeded = true;
                args[argIndex] = record[field];
                argIndex++;

                sqlBuilder.Add(DataProvider.Dialect.QuoteForColumnName(field));
                sqlBuilder.Add("=");
                sqlBuilder.Add(Parameter.Placeholder);
            }

            sqlBuilder.Add(" where ");
            sqlBuilder.Add(QuotedIdColumn);
            sqlBuilder.Add("=");
            sqlBuilder.Add(id.ToString());
            sqlBuilder.Add(" and ");
            sqlBuilder.Add(GetVersionExpression(record));

            var sql = sqlBuilder.ToSqlString();
            var rowsAffected = ctx.DBContext.Execute(sql, args);
            return rowsAffected;
        }

        private void NodeMoveTo(ITransactionContext ctx, long nodeLeft, long nodeRight, long nodeWidth, long newParentID)
        {
            //TODO 检查父节点不存在的异常
            long insertPos = 0;

            if (newParentID > 0)
            {
                var ids = new long[] { newParentID };
                var newParent = this.ReadInternal(ctx, ids, SearchParentNodeFields).First();
                insertPos = (long)newParent[LeftFieldName] + 1;
            }
            else
            {
                insertPos = 1;
            }

            //检查递归
            if (nodeLeft < insertPos && insertPos <= nodeRight)
            {
                throw new Exceptions.DataException("Cannot create a recursion node");
            }

            string sql;

            //nested-set 的算法就是把一个二维树转换成一维的线段
            sql = String.Format(
                "update {0} set _left=_left+{1} where _left>={2}",
                this.quotedTableName, nodeWidth, insertPos);
            ctx.DBContext.Execute(SqlString.Parse(sql));

            sql = String.Format(
                "update {0} set _right=_right+{1} where _right>={2}",
                this.quotedTableName, nodeWidth, insertPos);
            ctx.DBContext.Execute(SqlString.Parse(sql));

            if (nodeLeft < insertPos) //往左移动
            {
                long moveDistance = insertPos - nodeLeft;
                sql = String.Format(
                    "update {0} set _left=_left+{1}, _right=_right+{1} where _left>={2} and _left<{3}",
                    this.quotedTableName, moveDistance, nodeLeft, nodeRight);
                ctx.DBContext.Execute(SqlString.Parse(sql));
            }
            else //往右移动
            {
                long moveDistance = nodeLeft - insertPos;
                sql = String.Format(
                    "update {0} set _left=_left-{1}, _right=_right-{1} where _left>={2} and _left<{3}",
                    this.quotedTableName, moveDistance + nodeWidth, nodeLeft + nodeWidth, nodeRight + nodeWidth);
                ctx.DBContext.Execute(SqlString.Parse(sql));
            }

            //最后一步不需要执行了，之前已经更新过 parent
        }

        private void SetModificationInfo(ITransactionContext scope, IRecord record)
        {
            //处理最近更新用户与最近更新时间字段            
            if (this.ContainsField(UpdatedTimeFieldName))
            {
                record[UpdatedTimeFieldName] = DateTime.Now;
            }
            if (this.ContainsField(UpdatedUserFieldName) &&
                scope.Session != null &&
                scope.Session.UserId > 0)
            {
                record[UpdatedUserFieldName] = scope.Session.UserId;
            }
        }

        private void PrewriteManyToManyFields(
            ITransactionContext scope, long id, IRecord record, ICollection<string> allFields)
        {
            //过滤所有可以更新的 many2many 字段
            var writableManyToManyFields =
                from fn in allFields
                let f = this.Fields[fn]
                where f.Type == FieldType.ManyToMany && !f.IsReadonly && !f.IsFunctional
                select f;

            foreach (var f in writableManyToManyFields)
            {
                //中间表
                PrewriteManyToManyField(scope, id, record, f);
            }
        }

        private static void PrewriteManyToManyField(ITransactionContext scope, long id, IRecord record, IField f)
        {
            var relModel = (IModel)scope.GetResource(f.Relation);
            var constraints = new object[][]  
            { 
                new object[] { f.OriginField, "=", id }
            };

            //删掉原来的中间表记录重新插入
            var relIds = relModel.SearchInternal(scope, constraints, null, 0, 0);
            if (relIds.Length > 0)
            {
                relModel.DeleteInternal(scope, relIds);

                var targetIds = (long[])record[f.Name];
                foreach (var targetId in targetIds)
                {
                    var relRecord = new Record(2);
                    relRecord[f.OriginField] = id;
                    relRecord[f.RelatedField] = targetId;
                    relModel.CreateInternal(scope, relRecord);
                }
            }
        }

        private void PrewriteBaseModels(ITransactionContext ctx, IRecord record, IRecord existedRecord)
        {
            //处理继承表的策略
            //继承表写入的策略是这样的：
            //1. 先考察用户提供的字段，并按基类、子类的各个表分组，
            //      如果某一字段同时出现在基类和子类中的时候就报错
            //2. 分别更新各个基表
            //3. 最后更新子表
            foreach (var inheritInfo in this.Inheritances)
            {
                var baseModel = (IModel)ctx.GetResource(inheritInfo.BaseModel);
                var relatedFieldValue = existedRecord[inheritInfo.RelatedField] as object[];
                if (relatedFieldValue == null)
                {
                    var msg = String.Format("The field [{0}.{1}] can not be null",
                        this.Name, inheritInfo.RelatedField);
                    throw new Exceptions.DataException(msg);
                }
                var baseId = (long)(relatedFieldValue.First());

                //看用户提供的记录的字段是否涉及到基类
                var baseFields = baseModel.Fields.Keys.Intersect(record.Keys);
                if (baseFields.Count() > 0)
                {
                    var baseRecord = record.Where(p => baseFields.Contains(p.Key))
                        .ToDictionary(p => p.Key, p => p.Value);
                    baseModel.WriteInternal(ctx, baseId, baseRecord);
                }
            }
        }

        private void VerifyRecordVersion(long id, IRecord userRecord, IRecord existedRecord)
        {
            if (userRecord.ContainsKey(VersionFieldName))
            {
                var existedVersion = (long)existedRecord[VersionFieldName];
                var userVersion = (long)userRecord[VersionFieldName];
                if (existedVersion != userVersion)
                {
                    var msg = string.Format("不能更新 ['{0}', {1}]，因为其已经被其它用户更新",
                        this.TableName, id);
                    throw new ConcurrencyException(msg);
                }
            }
        }

        private static SqlString GetVersionExpression(IRecord record)
        {
            //如果存在 _version 字段就加入版本检查条件
            //TODO: 是否强制要求客户端必须送来 _version 字段？

            SqlString verExp = null;
            if (record.ContainsKey(VersionFieldName))
            {
                var version = (long)record[VersionFieldName];
                verExp = new SqlString(
                    DataProvider.Dialect.QuoteForColumnName(VersionFieldName),
                    "=",
                    version.ToString()); //现存数据库的版本必须比用户提供的版本相同

                //版本号也必须更新
                record[VersionFieldName] = version + 1;
            }
            else
            {
                verExp = new SqlString(DataProvider.Dialect.ToBooleanValueString(true));
            }
            return verExp;
        }
    }
}
