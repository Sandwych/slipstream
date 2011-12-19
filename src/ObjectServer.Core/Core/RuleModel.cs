using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;

using NHibernate.SqlCommand;

using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using IronRuby;

using ObjectServer.Model;
using ObjectServer.Runtime;

namespace ObjectServer.Core
{
    [Resource]
    public sealed class RuleModel : AbstractSqlModel
    {
        private static readonly Criterion[][] EmptyConstraints = new Criterion[][] { };

        public RuleModel()
            : base("core.rule")
        {
            Fields.Chars("name").SetLabel("Name").Required();
            Fields.ManyToOne("model", "core.model").Required().SetLabel("Model");
            Fields.Boolean("global").SetLabel("Global")
                .Required().SetDefaultValueGetter(s => true);
            Fields.Boolean("on_create").SetLabel("Apply for Creation")
               .Required().SetDefaultValueGetter(s => true);
            Fields.Boolean("on_read").SetLabel("Apply for Reading")
                .Required().SetDefaultValueGetter(s => true);
            Fields.Boolean("on_write").SetLabel("Apply for Writing")
                .Required().SetDefaultValueGetter(s => true);
            Fields.Boolean("on_delete").SetLabel("Apply for Deleting")
                .Required().SetDefaultValueGetter(s => true);
            Fields.ManyToMany("roles", "core.rule_role", "rule", "role").SetLabel("Roles");
            Fields.Chars("constraint").Required().SetLabel("Constraint");
        }

        /// <summary>
        /// 获取指定模型方法的访问规则
        /// 此方法把数据库里的规则表达式转换成查询约束
        /// </summary>
        /// <param name="model"></param>
        /// <param name="ctx"></param>
        /// <param name="modelName"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        internal static IList<Criterion[]> GetRuleConstraints(
            IServiceContext ctx, string modelName, string action)
        {
            Debug.Assert(ctx != null);
            Debug.Assert(!string.IsNullOrEmpty(modelName));
            Debug.Assert(!string.IsNullOrEmpty(action));
            Debug.Assert(action == "read" || action == "write" || action == "delete" || action == "create");

            var sql = new SqlString(
                @"select distinct ""r"".""_id"", ""r"".""name"", ""r"".""constraint"" from ""core_rule"" ""r"" ",
                @"inner join ""core_model"" ""m"" on (""r"".""model"" = ""m"".""_id"") ",
                @"where ""m"".""name""=", Parameter.Placeholder,
                    @" and (""r"".""on_" + action + @""" = '1') and ((""r"".""global"" = '1') or (""r"".""_id"" in ",
                    @"(select ""rr"".""rule"" from ""core_rule_role_rel"" ""rr"" ",
                        @"inner join ""core_user_role_rel"" ""ur"" on (""rr"".""role"" = ""ur"".""role"") ",
                        @"where ""ur"".""user"" =", Parameter.Placeholder, @" )))");

            var result = ctx.DataContext.QueryAsDictionary(sql, modelName, ctx.UserSession.UserId);

            if (result.Length > 0)
            {
                return ConvertConstraints(ctx, result);
            }
            else
            {
                return EmptyConstraints;
            }
        }

        private static IList<Criterion[]> ConvertConstraints(IServiceContext ctx, Dictionary<string, object>[] result)
        {
            var userModel = (UserModel)ctx.GetResource("core.user");
            dynamic user = userModel.Browse(ctx.UserSession.UserId);

            var evaluator = ctx.RuleConstraintEvaluator;
            evaluator.SetVariable("user", user);

            var constraints = new List<Criterion[]>();
            var cr = new List<Criterion>(4);
            foreach (var row in result)
            {
                cr.Clear();
                var constraintExp = (string)row["constraint"];
                var ruleObj = evaluator.Evaluate(constraintExp);

                foreach (dynamic d in ruleObj)
                {
                    var c = new Criterion((string)d[0], (string)d[1], d[2]);
                    cr.Add(c);
                }
                constraints.Add(cr.ToArray());
            }
            return constraints;
        }

        /// <summary>
        /// 获取指定模型方法的访问规则
        /// 此方法同 GetRuleDomain，但是经过了缓存
        /// </summary>
        /// <param name="scope"></param>
        /// <param name="modelName"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        internal static Criterion[] GetRuleConstraintsCached(IServiceContext scope, string modelName, string action)
        {
            throw new NotImplementedException();
        }
    }
}
