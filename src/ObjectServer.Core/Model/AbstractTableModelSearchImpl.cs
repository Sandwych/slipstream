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
using ObjectServer.Sql;

namespace ObjectServer.Model
{
    public abstract partial class AbstractTableModel : AbstractModel
    {
        private static readonly List<ConstraintExpression[]> EmptyRules =
            new List<ConstraintExpression[]>();
        private static readonly ConstraintExpression[] EmptyConstraints = { };

        public override long[] SearchInternal(
            ITransactionContext tc, object[] constraints, OrderExpression[] order, long offset, long limit)
        {
            if (tc == null)
            {
                throw new ArgumentNullException("scope");
            }

            this.VerifyReadPermission(tc);

            var translator = new ConstraintTranslator(tc, this);
            this.TranslateConstraints(tc, constraints, translator);

            //处理排序
            if (order != null)
            {
                translator.SetOrders(order);
            }

            var querySql = translator.ToSqlString();

            if (limit > 0) //处理数量限制
            {
                querySql = DataProvider.Dialect.GetLimitString(
                    querySql, new SqlString(offset.ToString()), new SqlString(limit.ToString()));
            }

            return tc.DBContext.QueryAsArray<long>(querySql, translator.Values);
        }


        public override long CountInternal(ITransactionContext tx, object[] constraints = null)
        {
            if (tx == null)
            {
                throw new ArgumentNullException("scope");
            }

            this.VerifyReadPermission(tx);

            var translator = new ConstraintTranslator(tx, this);
            this.TranslateConstraints(tx, constraints, translator);

            var querySql = translator.ToSqlString(true);

            return (long)tx.DBContext.QueryValue(querySql, translator.Values);
        }

        private void TranslateConstraints(ITransactionContext tc, object[] constraints, ConstraintTranslator translator)
        {
            //处理查询约束
            IEnumerable<ConstraintExpression> userConstraints = null;
            if (constraints != null)
            {
                userConstraints = constraints.Select(o => new ConstraintExpression(o));
            }
            else
            {
                userConstraints = EmptyConstraints;
            }

            try
            {
                translator.AddConstraints(userConstraints);
            }
            catch (ArgumentException)
            {
                throw new ArgumentException("Failed to translate constraints", "constraints");
            }

            translator.AddWhereFragment(new SqlString(" and "));

            //处理 Rule 约束
            this.GenerateReadingRuleConstraints(tc, translator);
        }


        private void GenerateReadingRuleConstraints(ITransactionContext scope, ConstraintTranslator translator)
        {
            Debug.Assert(scope != null);
            Debug.Assert(translator != null);

            //系统用户不检查访问规则
            if (scope.Session.IsSystemUser)
            {
                translator.AddGroupedConstraints(EmptyRules);
            }
            else
            {
                var ruleConstraints = RuleModel.GetRuleConstraints(scope, this.Name, "read");
                translator.AddGroupedConstraints(ruleConstraints);
            }
        }

    }
}
