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

            //处理版本字段与基类继承
            if (userRecord.ContainsKey(VersionFieldName) || this.Inheritances.Count > 0)
            {
                /*
                select * from <TableName> where _id=?
                */
                var sql1 = new SqlString("select * from ",
                    DataProvider.Dialect.QuoteForTableName(this.TableName),
                    " where ",
                    DataProvider.Dialect.QuoteForColumnName(IDFieldName),
                    "=",
                    Parameter.Placeholder);

                var existedRecord = scope.Connection.QueryAsDictionary(sql1, id)[0];

                this.VerifyRecordVersion(id, userRecord, existedRecord);

                this.PrewriteBaseModels(scope, record, existedRecord);
            }

            var allFields = record.Keys; //记录中的所有字段

            //先写入 many-to-many 字段
            this.PrewriteManyToManyFields(scope, id, record, allFields);

            //处理最近更新用户与最近更新时间字段            
            if (this.ContainsField(ModifiedTimeFieldName))
            {
                record[ModifiedTimeFieldName] = DateTime.Now;
            }
            if (this.ContainsField(ModifiedUserFieldName) &&
                scope.Session != null &&
                scope.Session.UserId > 0)
            {
                record[ModifiedUserFieldName] = scope.Session.UserId;
            }

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
            sqlBuilder.Add(DataProvider.Dialect.QuoteForColumnName(IDFieldName));
            sqlBuilder.Add("=");
            sqlBuilder.Add(id.ToString());
            sqlBuilder.Add(" and ");
            sqlBuilder.Add(GetVersionExpression(record));

            var sql = sqlBuilder.ToSqlString();
            var rowsAffected = scope.Connection.Execute(sql, args);

            //检查更新结果
            if (rowsAffected != 1)
            {
                var msg = string.Format("不能更新 ['{0}', {1}]，因为其已经被其它用户更新",
                    this.TableName, id);
                throw new ConcurrencyException(msg);
            }


            if (this.LogWriting)
            {
                AuditLog(scope, (long)id, this.Label + " updated");
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
                var baseId = (long)existedRecord[inheritInfo.RelatedField];

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
