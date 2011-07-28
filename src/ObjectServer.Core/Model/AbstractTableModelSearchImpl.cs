using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Data;
using System.Reflection;
using System.Dynamic;

using ObjectServer.Core;
using ObjectServer.Data;
using ObjectServer.Utility;
using ObjectServer.SqlTree;

namespace ObjectServer.Model
{
    public abstract partial class AbstractTableModel : AbstractModel
    {
        private static readonly ConstraintExpression[] EmptyDomain = { };

        public override long[] SearchInternal(
            IServiceScope scope, object[] constraints = null, OrderExpression[] order = null, long offset = 0, long limit = 0)
        {
            if (!this.CanRead)
            {
                throw new NotSupportedException();
            }

            if (!scope.CanReadModel(scope.Session.UserId, this.Name))
            {
                throw new UnauthorizedAccessException("Access denied");
            }

            string mainTable = this.TableName;
            var mainTableAlias = "_t0";

            var selectBuilder = new SelectBuilder(mainTable, mainTableAlias);
            var parser = new ConstraintBuilder(scope, this, selectBuilder);

            var userConstraints = new List<ConstraintExpression>();
            if (constraints != null)
            {
                userConstraints.AddRange(from o in constraints select new ConstraintExpression(o));
            }

            var ruleExp = new BracketedExpression(RuleConstraintsToSqlExpression(scope, parser));
            var userExp = ValueExpression.TrueExpression;
            var joinExp = ValueExpression.TrueExpression;

            var exps = new IExpression[] { joinExp, ruleExp, userExp };

            //var selfFields = this.Fields.Where(p => p.Value.IsColumn()).Select(p => p.Key);
            var whereExp = exps.JoinExpressions(ExpressionOperator.AndOperator);

            //处理排序
            var orderbyClause = ConvertOrderExpression(order, mainTableAlias);

            var columnExps = new AliasExpressionList(new string[] { mainTableAlias + "." + IDFieldName });

            var joinClauses = selectBuilder.

            var mainTableAliasExp = new AliasExpression(mainTable, mainTableAlias);
            var select = new SelectStatement(
                columnExps, new FromClause(mainTableAliasExp), new WhereClause(whereExp));
            select.JoinClauses = joinClauses;
            select.DistinctClause = new DistinctClause(
                new IdentifierExpression[] { new IdentifierExpression("_t0._id") });

            select.OrderByClause = orderbyClause;
            if (offset > 0)
            {
                select.OffsetClause = new OffsetClause(offset);
            }

            if (limit > 0)
            {
                select.LimitClause = new LimitClause(limit);
            }

            var sv = new StringifierVisitor();
            select.Traverse(sv);
            var sql = sv.ToString();
            return scope.Connection.QueryAsArray<long>(sql);
        }

        private IExpression RuleConstraintsToSqlExpression(IServiceScope scope, ConstraintBuilder parser)
        {
            IExpression ruleExp = ValueExpression.TrueExpression;
            //安全：加入访问规则限制
            if (!scope.Session.IsSystemUser) //系统用户不检查访问规则
            {
                //每组之间使用 OR 连接，一组中的元素之间使用 AND 连接
                var ruleConstraints = RuleModel.GetRuleConstraints(scope, this.Name, "read");
                var groupExps = new List<IExpression>(ruleConstraints.Count);
                foreach (var ruleGroup in ruleConstraints)
                {
                    var groupConstraint = parser.Push(ruleGroup);
                    groupExps.Add(new BracketedExpression(groupConstraint));
                }

                if (groupExps.Count > 0)
                {
                    ruleExp = groupExps.JoinExpressions(ExpressionOperator.OrOperator);
                }
            }
            return ruleExp;
        }

        private static OrderbyClause ConvertOrderExpression(OrderExpression[] order, string mainTableAlias)
        {
            OrderbyClause orderbyClause = null;
            if (order != null && order.Length > 0)
            {
                var orderbyItems = order.Select(
                    o => new OrderbyItem(mainTableAlias + "." + o.Field, o.Order.ToUpperString()));
                orderbyClause = new OrderbyClause(orderbyItems);
            }
            else
            {
                orderbyClause = new OrderbyClause(mainTableAlias + "." + IDFieldName, "ASC");
            }
            return orderbyClause;
        }

    }
}
