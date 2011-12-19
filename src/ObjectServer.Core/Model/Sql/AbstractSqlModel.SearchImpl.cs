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
using NHibernate.SqlTypes;

using ObjectServer.Core;
using ObjectServer.Data;

namespace ObjectServer.Model
{
    public abstract partial class AbstractSqlModel : AbstractModel
    {
        private static readonly List<Criterion[]> EmptyRules =
            new List<Criterion[]>();
        private static readonly Criterion[] EmptyConstraint = { };

        public override long[] SearchInternal(object[] constraint, OrderExpression[] order, long offset, long limit)
        {
            var ctx = this.DbDomain.CurrentSession;
            var translator = new SqlQueryBuilder(ctx, this);
            this.TranslateConstraint(ctx, constraint, translator);

            //处理排序
            if (order != null)
            {
                translator.SetOrders(order);
            }
            else
            {
                translator.SetOrders(this.Order);
            }

            var querySql = translator.ToSqlString();

            if (limit > 0) //处理数量限制
            {
                querySql = DataProvider.Dialect.GetLimitString(
                    querySql, new SqlString(offset.ToString()), new SqlString(limit.ToString()));
            }

            return ctx.DataContext.QueryAsArray<long>(querySql, translator.Values);
        }


        public override long CountInternal(object[] constraint)
        {
            var ctx = this.DbDomain.CurrentSession;

            var translator = new SqlQueryBuilder(ctx, this);
            this.TranslateConstraint(ctx, constraint, translator);

            var querySql = translator.ToSqlString(true);

            return Convert.ToInt64(ctx.DataContext.QueryValue(querySql, translator.Values));
        }

        private void TranslateConstraint(IServiceContext tc, object[] constraint, SqlQueryBuilder translator)
        {
            //处理查询约束
            IEnumerable<Criterion> userConstraint = null;
            if (constraint != null)
            {
                userConstraint = constraint.Select(o => new Criterion(o));
            }
            else
            {
                userConstraint = EmptyConstraint;
            }

            translator.AddCriteria(userConstraint);

            translator.AddWhereFragment(new SqlString(" and "));

            //处理 Rule 约束
            this.GenerateReadingRuleConstraints(tc, translator);
        }


        private void GenerateReadingRuleConstraints(IServiceContext scope, SqlQueryBuilder translator)
        {
            Debug.Assert(scope != null);
            Debug.Assert(translator != null);

            //系统用户不检查访问规则
            if (scope.UserSession.IsSystemUser)
            {
                translator.AddConstraint(EmptyRules);
            }
            else
            {
                var ruleConstraints = RuleModel.GetRuleConstraints(scope, this.Name, "read");
                translator.AddConstraint(ruleConstraints);
            }
        }

    }
}
