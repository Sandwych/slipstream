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
        private static readonly DomainExpression[] EmptyDomain = { };

        public override long[] SearchInternal(
            IServiceScope scope, object[] domain = null, OrderExpression[] order = null, long offset = 0, long limit = 0)
        {
            if (!this.CanRead)
            {
                throw new NotSupportedException();
            }

            if (!scope.CanReadModel(scope.Session.UserId, this.Name))
            {
                throw new UnauthorizedAccessException("Access denied");
            }

            var domainInternal = new List<DomainExpression>();
            if (domain != null)
            {
                domainInternal.AddRange(from o in domain select new DomainExpression(o));
            }

            //安全：加入访问规则限制
            if (!scope.Session.IsSystemUser) //系统用户不检查访问规则
            {
                var ruleDomain = RuleModel.GetRuleDomain(scope, this.Name, "read");
                domainInternal.AddRange(ruleDomain);
            }

            string mainTable = this.TableName;

            var columnExps = new AliasExpressionList(new string[] { mainTable + "." + IDFieldName });

            OrderbyClause orderbyClause = null;

            if (order != null && order.Length > 0)
            {
                var orderbyItems = order.Select(
                    o => new OrderbyItem(mainTable + "." + o.Field, o.Order.ToUpperString()));
                orderbyClause = new OrderbyClause(orderbyItems);
            }
            else
            {
                orderbyClause = new OrderbyClause(mainTable + "." + IDFieldName, "ASC");
            }

            var selfFields = this.Fields.Where(p => p.Value.IsColumn()).Select(p => p.Key);
            var parser = new DomainParser(scope, this);
            var parsedResult = parser.Parse(domainInternal);
            var select = new SelectStatement(
                columnExps, new FromClause(parsedResult.Item1), new WhereClause(parsedResult.Item2));

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

    }
}
