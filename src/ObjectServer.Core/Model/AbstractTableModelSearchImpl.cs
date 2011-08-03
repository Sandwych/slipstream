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

            var translator = new ConstraintTranslator(scope, this);

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
            translator.AddConstraints(userConstraints);
            translator.AddWhereFragment(new SqlString(" and "));

            //处理 Rule 约束
            this.GenerateReadingRuleConstraints(scope, translator);

            //处理排序
            if (order != null)
            {
                translator.SetOrders(order);
            }

            //TODO 处理 Rules
            var querySql = translator.ToSqlString();

            if (limit > 0) //处理数量限制
            {
                querySql = DataProvider.Dialect.GetLimitString(
                    querySql, new SqlString(offset.ToString()), new SqlString(limit.ToString()));
            }

            return scope.Connection.QueryAsArray<long>(querySql, translator.Values);
        }

        private void GenerateReadingRuleConstraints(IServiceScope scope, ConstraintTranslator translator)
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
