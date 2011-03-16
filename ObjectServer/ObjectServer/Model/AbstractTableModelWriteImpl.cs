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
        public override void WriteInternal(
            IResourceScope ctx, long id, IDictionary<string, object> userRecord)
        {
            if (!this.CanWrite)
            {
                throw new NotSupportedException();
            }

            var record = new Dictionary<string, object>(userRecord);
            record["id"] = id;

            //处理最近更新用户与最近更新时间字段            
            if (this.ContainsField(ModifiedTimeFieldName))
            {
                record[ModifiedTimeFieldName] = DateTime.Now;
            }
            if (this.ContainsField(ModifiedUserFieldName))
            {
                record[ModifiedUserFieldName] = ctx.Session.UserId;
            }

            var allFields = record.Keys; //记录中的所有字段
            //所有可更新的字段
            var updatableColumnFields = allFields.Where(
                f => this.Fields[f].IsColumn() &&
                    !this.Fields[f].IsReadonly &&
                    this.Fields[f].Name != "id"
                ).ToArray();
            this.ConvertFieldToColumn(ctx, record, updatableColumnFields);

            //处理继承表的策略

            //检查字段

            var columns = new List<IBinaryExpression>(record.Count);
            int i = 0;
            var args = new List<object>(record.Count);
            foreach (var field in updatableColumnFields)
            {
                var exp = new BinaryExpression(
                    new IdentifierExpression(field),
                    ExpressionOperator.EqualOperator,
                    new ParameterExpression("@" + i.ToString()));
                columns.Add(exp);
                args.Add(record[field]);

                ++i;
            }

            var whereExp = new BinaryExpression(
                new AliasExpression("id"),
                ExpressionOperator.EqualOperator,
                new ValueExpression(id));


            //如果存在 _version 字段就加入版本检查条件
            //TODO: 是否强制要求客户端必须送来 _version 字段？
            if (record.ContainsKey(VersionFieldName))
            {
                var version = (long)record[VersionFieldName];
                var verExp = new BinaryExpression(
                    new AliasExpression(VersionFieldName),
                    ExpressionOperator.EqualOperator,
                    new ValueExpression(version)); //现存数据库的版本必须比用户提供的版本相同
                whereExp = new BinaryExpression(
                    whereExp,
                    ExpressionOperator.AndOperator,
                    verExp);

                //版本号也必须更新
                record[VersionFieldName] = version + 1;
            }

            var updateStatement = new UpdateStatement(
                new AliasExpression(this.TableName),
                new SetClause(columns),
                new WhereClause(whereExp));

            var sql = updateStatement.ToString();

            var rowsAffected = ctx.DatabaseProfile.DataContext.Execute(sql, args.ToArray());

            //检查更新结果
            if (rowsAffected != 1)
            {
                var msg = string.Format("不能更新 ['{0}', {1}]，因为其已经被其它用户更新",
                    this.TableName, id);
                throw new ConcurrencyException(msg);
            }

            if (this.LogWriting)
            {
                AuditLog(ctx, (long)id, this.Label + " updated");
            }
        }
    }
}
