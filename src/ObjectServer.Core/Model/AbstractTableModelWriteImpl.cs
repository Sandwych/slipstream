using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Data;
using System.Reflection;
using System.Dynamic;

using NHibernate.SqlCommand;

using ObjectServer.Exceptions;
using ObjectServer.Data;
using ObjectServer.Utility;

namespace ObjectServer.Model
{
    public abstract partial class AbstractTableModel : AbstractModel
    {
        public override void WriteInternal(
            IServiceScope scope, long id, IDictionary<string, object> userRecord)
        {
            if (!this.CanWrite)
            {
                throw new NotSupportedException();
            }

            if (!scope.CanWriteModel(scope.Session.UserId, this.Name))
            {
                throw new UnauthorizedAccessException("Access denied");
            }

            var record = ClearUserRecord(userRecord);
            var isParentChanged = false;
            var oldParentID = (long)0;
            Dictionary<string, object> existedRecord = null;

            //处理版本字段与基类继承
            if (userRecord.ContainsKey(VersionFieldName) || this.Inheritances.Count > 0 || this.Hierarchy)
            {
                /*
                select * from <TableName> where _id=?
                */
                var sqlSelectSelf = String.Format("select * from {0} where _id = ?", this.quotedTableName);
                existedRecord = scope.DBContext.QueryAsDictionary(SqlString.Parse(sqlSelectSelf), id)[0];

                this.VerifyRecordVersion(id, userRecord, existedRecord);

                this.PrewriteBaseModels(scope, record, existedRecord);

                if (userRecord.ContainsKey(ParentFieldName))
                {
                    oldParentID = (long)existedRecord[ParentFieldName];
                    var newParentId = (long)userRecord[ParentFieldName];
                    if (newParentId != oldParentID)
                    {
                        isParentChanged = true;
                    }
                }
            }

            var allFields = record.Keys; //记录中的所有字段

            //先写入 many-to-many 字段
            this.PrewriteManyToManyFields(scope, id, record, allFields);

            this.SetModificationInfo(scope, record);

            //所有可更新的字段
            var updatableColumnFields = allFields.Where(
                f => this.Fields[f].IsColumn() && !this.Fields[f].IsReadonly).ToArray();
            this.ConvertFieldToColumn(scope, record, updatableColumnFields);

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
            var rowsAffected = scope.DBContext.Execute(sql, args);

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
                var fields = new string[] { IDFieldName, LeftFieldName, RightFieldName };
                var nodeLeft = (long)existedRecord[LeftFieldName];
                var nodeRight = (long)existedRecord[RightFieldName];
                var nodeWidth = nodeRight - nodeLeft + 1;
                var newParentID = (long)userRecord[ParentFieldName];
                var newParent = this.ReadInternal(scope, new long[] { newParentID }, fields)[0];
                var newParentLeft = (long)newParent[LeftFieldName];
                var newParentRight = (long)newParent[RightFieldName];

                scope.DBContext.LockTable(this.TableName);

                //# step 1: temporary "remove" moving node
                //
                //UPDATE `list_items`
                //SET `pos_left` = 0-(`pos_left`), `pos_right` = 0-(`pos_right`)
                //WHERE `pos_left` >= @node_pos_left AND `pos_right` <= @node_pos_right;
                var sqlStr = String.Format(
                    "update {0} set _left = 0 - _left, _right = 0 - _right " +
                    "where _left >= {1} and _right <= {2}",
                    this.quotedTableName, nodeLeft, nodeRight);
                scope.DBContext.Execute(SqlString.Parse(sqlStr));

                //# step 2: decrease left and/or right position values of currently 'lower' items (and parents)
                //
                //UPDATE `list_items`
                //SET `pos_left` = `pos_left` - @node_size
                //WHERE `pos_left` > @node_pos_right;
                //UPDATE `list_items`
                //SET `pos_right` = `pos_right` - @node_size
                //WHERE `pos_right` > @node_pos_right;
                sqlStr = String.Format("update {0} set _left = _left - {1} where _left > {2}",
                    this.quotedTableName, nodeWidth, nodeRight);
                scope.DBContext.Execute(SqlString.Parse(sqlStr));
                sqlStr = String.Format("update {0} set _right = _right - {1} where _right > {2}",
                    this.quotedTableName, nodeWidth, nodeRight);
                scope.DBContext.Execute(SqlString.Parse(sqlStr));

                /* # step 3: increase left and/or right position values of future 'lower' items (and parents)
                    UPDATE `list_items`
                    SET `pos_left` = `pos_left` + @node_size
                    WHERE `pos_left` >= IF(@parent_pos_right > @node_pos_right, 
                        @parent_pos_right - @node_size, @parent_pos_right);
                    UPDATE `list_items`
                    SET `pos_right` = `pos_right` + @node_size
                    WHERE `pos_right` >= IF(@parent_pos_right > @node_pos_right, 
                        @parent_pos_right - @node_size, @parent_pos_right);
                */

                sqlStr = String.Format(
                    "update {0} set _left = _left + {1} where _left >= " +
                    "case when {2} > {3} then {4} - {5} else {6} end",
                    this.quotedTableName, nodeWidth, newParentRight, 
                    nodeRight, newParentRight, nodeWidth, newParentRight);
                scope.DBContext.Execute(SqlString.Parse(sqlStr));

                sqlStr = String.Format(
                    "update {0} set _right = _right + {1} where _right >= " +
                    "case when {2} > {3} then {4} - {5} else {6} end",
                    this.quotedTableName, nodeWidth, newParentRight, 
                    nodeRight, newParentRight, nodeWidth, newParentRight);
                scope.DBContext.Execute(SqlString.Parse(sqlStr));
                
                /* # step 4: move node (ant it's subnodes) and update it's parent item id
                    UPDATE `list_items`
                    SET
                        `pos_left` = 0-(`pos_left`)+
                            IF(@parent_pos_right > @node_pos_right, 
                                @parent_pos_right - @node_pos_right - 1, 
                                @parent_pos_right - @node_pos_right - 1 + @node_size),
                        `pos_right` = 0-(`pos_right`)+
                            IF(@parent_pos_right > @node_pos_right, 
                                @parent_pos_right - @node_pos_right - 1, 
                                @parent_pos_right - @node_pos_right - 1 + @node_size)
                    WHERE `pos_left` <= 0-@node_pos_left AND `pos_right` >= 0-@node_pos_right;
                    UPDATE `list_items`
                    SET `parent_item_id` = @parent_id
                    WHERE `item_id` = @node_id;
                */

                sqlStr = String.Format(
                    "update {0} set " +
                    "_left = 0 - _left + case when {1} > {2} then {1} - {2} - 1 else {1} - {2} - 1 + {3} end, " +
                    "_right = 0 - _right + case when {1} > {2} then {1} - {2} - 1 else {1} - {2} - 1 + {3} end " +
                    "where _left <= 0 - {4} and _right >= 0 - {2} ",
                    this.quotedTableName, newParentRight, nodeRight, nodeWidth, nodeLeft);
                scope.DBContext.Execute(SqlString.Parse(sqlStr));

                //最后一步不需要执行了，之前已经更新过 parent
            }

            if (this.LogWriting)
            {
                AuditLog(scope, (long)id, this.Label + " updated");
            }
        }

        private void SetModificationInfo(IServiceScope scope, Dictionary<string, object> record)
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
            IServiceScope scope, long id, Dictionary<string, object> record, ICollection<string> allFields)
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

        private static void PrewriteManyToManyField(IServiceScope scope, long id, Dictionary<string, object> record, IField f)
        {
            var relModel = (IModel)scope.GetResource(f.Relation);
            var constraints = new object[][]  
            { 
                new object[] { f.OriginField, "=", id }
            };

            //删掉原来的中间表记录重新插入
            var relIds = relModel.SearchInternal(scope, constraints);
            if (relIds.Length > 0)
            {
                relModel.DeleteInternal(scope, relIds);

                var targetIds = (long[])record[f.Name];
                foreach (var targetId in targetIds)
                {
                    var relRecord = new Dictionary<string, object>(2);
                    relRecord[f.OriginField] = id;
                    relRecord[f.RelatedField] = targetId;
                    relModel.CreateInternal(scope, relRecord);
                }
            }
        }

        private void PrewriteBaseModels(IServiceScope ctx, Dictionary<string, object> record, Dictionary<string, object> existedRecord)
        {
            //处理继承表的策略
            //继承表写入的策略是这样的：
            //1. 先考察用户提供的字段，并按基类、子类的各个表分组，
            //      如果某一字段同时出现在基类和子类中的时候就报错
            //2. 分别更新各个基类表
            //3. 最后更新子类表
            foreach (var inheritInfo in this.Inheritances)
            {
                var baseModel = (IModel)ctx.GetResource(inheritInfo.BaseModel);
                var baseID = (long)existedRecord[inheritInfo.RelatedField];

                //看用户提供的记录的字段是否涉及到基类
                var baseFields = baseModel.Fields.Keys.Intersect(record.Keys);
                if (baseFields.Count() > 0)
                {
                    var baseRecord = record.Where(p => baseFields.Contains(p.Key))
                        .ToDictionary(p => p.Key, p => p.Value);
                    baseModel.WriteInternal(ctx, baseID, baseRecord);
                }
            }
        }

        private void VerifyRecordVersion(long id, IDictionary<string, object> userRecord, Dictionary<string, object> existedRecord)
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

        private static SqlString GetVersionExpression(IDictionary<string, object> record)
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
