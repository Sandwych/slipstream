using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ObjectServer.SqlTree;

namespace ObjectServer.Model
{
    internal sealed class ModelQuery
    {
        private IModel model;
        private IContext context;

        public ModelQuery(IContext ctx, IModel model)
        {
            this.context = ctx;
            this.model = model;
        }

        public object[] Search(object[] domain, long offset, long limit)
        {
            if (domain == null)
            {
                throw new ArgumentNullException("exp");
            }

            var fields = domain.Select(d => (string)((object[])d)[0]);
            var parser = new DomainParser(this.model, domain);
            var columnExps = new AliasExpressionList(new string[] { "id" });
            var whereExp = parser.ToExpressionTree();
            var select = new SelectStatement(
                columnExps,
                new FromClause(new string[] { model.TableName }),
                new WhereClause(whereExp));

            if (offset > 0)
            {
                select.OffsetClause = new OffsetClause(offset);
            }

            if (limit > 0)
            {
                select.LimitClause = new LimitClause(limit);
            }

            //TODO: 这里检查权限等，处理查询非表中字段等

            /* //自动添加 active 字段
            if (this.model.ContainsField(ModelBase.ActiveFieldName)
                && !fields.Contains(ModelBase.ActiveFieldName))
            {
                exp.AddExpression(new object[] { ModelBase.ActiveFieldName, "=", true });
            }
             * */

            var sv = new StringifierVisitor();
            select.Traverse(sv);
            var sql = sv.ToString();

            using (var cmd = this.context.Database.DataContext.Connection.CreateCommand())
            {
                cmd.CommandText = sql;
                using (var reader = cmd.ExecuteReader())
                {
                    var result = new List<object>();
                    while (reader.Read())
                    {
                        result.Add(reader.GetInt64(0));
                    }
                    return result.ToArray();
                }
            }
        }
    }
}
