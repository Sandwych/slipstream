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

namespace ObjectServer.Model {
    public abstract partial class AbstractSqlModel : AbstractModel {
        private static readonly List<Criterion[]> EmptyRules =
            new List<Criterion[]>();
        private static readonly Criterion[] EmptyConstraint = { };

        public override long[] SearchInternal(object[] constraint, OrderExpression[] order, long offset, long limit) {
            var ctx = this.DbDomain.CurrentSession;
            var translator = new SqlQueryBuilder(ctx, this);
            this.TranslateConstraint(constraint, translator);

            //处理排序
            if (order != null) {
                translator.SetOrders(order);
            }
            else {
                translator.SetOrders(this.Order);
            }

            var querySql = translator.ToSqlString();

            if (limit > 0) //处理数量限制
            {
                querySql = this.DbDomain.DataProvider.Dialect.GetLimitString(
                    querySql, new SqlString(offset.ToString()), new SqlString(limit.ToString()));
            }

            return ctx.DataContext.QueryAsArray<long>(querySql, translator.Values);
        }


        public override long CountInternal(object[] constraint) {
            var ctx = this.DbDomain.CurrentSession;

            var translator = new SqlQueryBuilder(ctx, this);
            this.TranslateConstraint(constraint, translator);

            var querySql = translator.ToSqlString(true);

            return Convert.ToInt64(ctx.DataContext.QueryValue(querySql, translator.Values));
        }

        private void TranslateConstraint(object[] constraint, SqlQueryBuilder translator) {
            //处理查询约束
            IEnumerable<Criterion> userConstraint = null;
            if (constraint != null) {
                userConstraint = constraint.Select(o => new Criterion(o));
            }
            else {
                userConstraint = EmptyConstraint;
            }

            translator.AddCriteria(userConstraint);

            translator.AddWhereFragment(new SqlString(" and "));

            //处理 Rule 约束
            this.GenerateReadingRuleConstraints(translator);
        }


        private void GenerateReadingRuleConstraints(SqlQueryBuilder translator) {
            Debug.Assert(translator != null);
            var ctx = this.DbDomain.CurrentSession;

            //系统用户不检查访问规则
            if (ctx.UserSession.IsSystemUser) {
                translator.AddConstraint(EmptyRules);
            }
            else {
                var ruleConstraints = RuleModel.GetRuleConstraints(ctx, this.Name, "read");
                translator.AddConstraint(ruleConstraints);
            }
        }

    }
}
