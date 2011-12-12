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
using ObjectServer.Utility;

namespace ObjectServer.Model
{
    public abstract partial class AbstractSqlModel : AbstractModel
    {
        private static readonly List<Criterion[]> EmptyRules =
            new List<Criterion[]>();
        private static readonly Criterion[] EmptyConstraint = { };

        public override long[] SearchInternal(
            IServiceContext tc, object[] constraint, OrderExpression[] order, long offset, long limit)
        {
            if (tc == null)
            {
                throw new ArgumentNullException("svcCtx");
            }

            var translator = new ConstraintTranslator(tc, this);
            this.TranslateConstraint(tc, constraint, translator);

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

            return tc.DBContext.QueryAsArray<long>(querySql, translator.Values);
        }


        public override long CountInternal(IServiceContext tx, object[] constraint)
        {
            if (tx == null)
            {
                throw new ArgumentNullException("svcCtx");
            }

            var translator = new ConstraintTranslator(tx, this);
            this.TranslateConstraint(tx, constraint, translator);

            var querySql = translator.ToSqlString(true);

            return Convert.ToInt64(tx.DBContext.QueryValue(querySql, translator.Values));
        }

        private void TranslateConstraint(IServiceContext tc, object[] constraint, ConstraintTranslator translator)
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


        private void GenerateReadingRuleConstraints(IServiceContext scope, ConstraintTranslator translator)
        {
            Debug.Assert(scope != null);
            Debug.Assert(translator != null);

            //系统用户不检查访问规则
            if (scope.Session.IsSystemUser)
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
