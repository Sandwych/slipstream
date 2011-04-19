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
        public override long[] SearchInternal(
            IServiceScope scope, object[][] domain = null, OrderInfo[] order = null, long offset = 0, long limit = 0)
        {
            if (!this.CanRead)
            {
                throw new NotSupportedException();
            }

            if (!ModelSecurity.CanReadModel(scope, scope.Session.UserId, this.Name))
            {
                throw new UnauthorizedAccessException("Access denied");
            }

            object[] domainInternal = domain;
            if (domain == null)
            {
                domainInternal = new object[][] { };
            }

            string mainTable = this.TableName;

            var selfFromExp = new AliasExpression(this.TableName, mainTable);

            var fields = domainInternal.Select(d => (string)((object[])d)[0]);

            var columnExps = new AliasExpressionList(new string[] { mainTable + ".id" });
            var select = new SelectStatement(columnExps, new FromClause(selfFromExp));

            OrderbyClause orderbyClause = null;

            if (order != null && order.Length > 0)
            {
                var orderbyItems = order.Select(
                    o => new OrderbyItem(mainTable + "." + o.Field, o.Order.ToUpperString()));
                orderbyClause = new OrderbyClause(orderbyItems);
            }
            else
            {
                orderbyClause = new OrderbyClause(mainTable + ".id", "ASC");
            }

            select.OrderByClause = orderbyClause;

            if (offset > 0)
            {
                select.OffsetClause = new OffsetClause(offset);
            }

            if (limit > 0)
            {
                select.LimitClause = new LimitClause(limit);
            }

            var selfFields = this.Fields.Where(p => p.Value.IsColumn()).Select(p => p.Key);


            //TODO: 这里检查过滤规则等，处理查询非表中字段等
            //TODO: 自动添加 active 字段
            //TODO 处理 childof 等复杂查询
            //继承查询的策略很简单，直接把基类表连接到查询里
            //如果有重复的字段，就以子类的字段为准
            if (this.Inheritances.Count > 0)
            {
                foreach (var d in domainInternal)
                {
                    string tableName = null;
                    var e = (object[])d;
                    var fieldName = (string)e[0];
                    var metaField = this.Fields[fieldName];

                    if (SystemReadonlyFields.Contains(fieldName))
                    {
                        tableName = this.TableName;
                    }
                    else
                    {
                        var tableNames =
                            from i in this.Inheritances
                            let bm = (AbstractTableModel)scope.GetResource(i.BaseModel)
                            where bm.Fields.ContainsKey(fieldName)
                            select bm.TableName;
                        tableName = tableNames.Single();
                    }

                    e[0] = tableName + '.' + fieldName;
                }

                foreach (var inheritInfo in this.Inheritances)
                {
                    var baseModel = (AbstractTableModel)scope.GetResource(inheritInfo.BaseModel);
                    var baseTableExp = new AliasExpression(baseModel.TableName);
                    select.FromClause.ExpressionCollection.Expressions.Add(baseTableExp);
                }
            }

            var parser = new DomainParser(this, domainInternal);
            var whereExp = parser.ToExpressionTree();

            select.WhereClause = new WhereClause(whereExp);

            var sv = new StringifierVisitor();
            select.Traverse(sv);
            var sql = sv.ToString();
            return scope.Connection.QueryAsArray<long>(sql);

            /*
            using (var cmd = scope.Connection.Connection.CreateCommand())
            {
                cmd.CommandText = sql;
                using (var reader = cmd.ExecuteReader())
                {
                    var result = new List<long>();
                    while (reader.Read())
                    {
                        result.Add(reader.GetInt64(0));
                    }
                    return result.ToArray();
                }
            }
            */
        }

    }
}
