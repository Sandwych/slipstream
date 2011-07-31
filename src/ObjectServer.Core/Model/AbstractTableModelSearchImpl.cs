﻿using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Data;
using System.Reflection;
using System.Dynamic;

using NHibernate.SqlCommand;

using ObjectServer.Core;
using ObjectServer.Data;
using ObjectServer.Utility;
using ObjectServer.SqlTree;
using ObjectServer.Sql;

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

            var translator = new ConstraintTranslator(scope, this);

            //处理查询约束
            foreach (var c in constraints)
            {
                translator.Add(new ConstraintExpression(c));
            }

            //处理排序
            if (order != null)
            {
                translator.SetOrders(order);
            }

            //TODO 处理 Rules
            var querySql = translator.ToSqlString();

            if (limit > 0) //处理数量限制
            {
                querySql = Data.DataProvider.Dialect.GetLimitString(
                    querySql, new SqlString(offset.ToString()), new SqlString(limit.ToString()));
            }

            using (var sqlCommand = Data.DataProvider.Driver.GenerateCommand(
                CommandType.Text, querySql, new NHibernate.SqlTypes.SqlType[] { }))
            {

                sqlCommand.Connection = scope.Connection.DBConnection;
                var sql = sqlCommand.CommandText;
                for (int i = 0; i < translator.Values.Length; i++)
                {
                    var value = translator.Values[i];
                    var param = sqlCommand.CreateParameter();
                    param.ParameterName = "p" + i.ToString();
                    param.Value = value;
                    sqlCommand.Parameters.Add(param);
                }

                using (var reader = sqlCommand.ExecuteReader())
                {
                    var result = new List<long>();
                    while (reader.Read())
                    {
                        result.Add((long)reader[0]);
                    }
                    return result.ToArray();
                }
            }
        }

        private void RuleConstraintsToSqlExpression(IServiceScope scope, ConstraintBuilderOld parser)
        {
            //安全：加入访问规则限制
            if (!scope.Session.IsSystemUser) //系统用户不检查访问规则
            {
                //每组之间使用 OR 连接，一组中的元素之间使用 AND 连接
                var ruleConstraints = RuleModel.GetRuleConstraints(scope, this.Name, "read");
                var groupExps = new List<IExpression>(ruleConstraints.Count);
                foreach (var ruleGroup in ruleConstraints)
                {
                    parser.Push(ruleGroup);
                }
            }
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
