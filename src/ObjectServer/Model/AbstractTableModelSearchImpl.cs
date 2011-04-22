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

            var parser = new DomainParser(scope, this, domainInternal);

            var selfFromExp = new AliasExpression(this.TableName, mainTable);
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

            var whereExp = parser.ToExpression();
            var fromClause = new FromClause(parser.Tables);
            select.WhereClause = new WhereClause(whereExp);
            select.FromClause = fromClause;

            var sv = new StringifierVisitor();
            select.Traverse(sv);
            var sql = sv.ToString();
            return scope.Connection.QueryAsArray<long>(sql);
        }

    }
}
