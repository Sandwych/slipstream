using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;

using NHibernate.SqlCommand;

using SlipStream.Entity;
using SlipStream.Runtime;

namespace SlipStream.Core
{
    [Resource]
    public sealed class RuleEntity : AbstractSqlEntity
    {
        private static readonly Criterion[][] EmptyConstraints = new Criterion[][] { };

        public RuleEntity() : base("core.rule")
        {
            Fields.Chars("name").WithLabel("Name").WithRequired();
            Fields.ManyToOne("meta_entity", "core.meta_entity").WithRequired().WithLabel("Meta Entity");
            Fields.Boolean("global").WithLabel("Global")
                .WithRequired().WithDefaultValueGetter(s => true);
            Fields.Boolean("on_create").WithLabel("Apply for Creation")
               .WithRequired().WithDefaultValueGetter(s => true);
            Fields.Boolean("on_read").WithLabel("Apply for Reading")
                .WithRequired().WithDefaultValueGetter(s => true);
            Fields.Boolean("on_write").WithLabel("Apply for Writing")
                .WithRequired().WithDefaultValueGetter(s => true);
            Fields.Boolean("on_delete").WithLabel("Apply for Deleting")
                .WithRequired().WithDefaultValueGetter(s => true);
            Fields.ManyToMany("roles", "core.rule_role", "rule", "role").WithLabel("Roles");
            Fields.Chars("constraint").WithRequired().WithLabel("Constraint");
        }

        /// <summary>
        /// 获取指定模型方法的访问规则
        /// 此方法把数据库里的规则表达式转换成查询约束
        /// </summary>
        /// <param name="model"></param>
        /// <param name="ctx"></param>
        /// <param name="entityName"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        internal static IList<Criterion[]> GetRuleConstraints(
            IServiceContext ctx, string entityName, string action)
        {
            Debug.Assert(ctx != null);
            Debug.Assert(!string.IsNullOrEmpty(entityName));
            Debug.Assert(!string.IsNullOrEmpty(action));
            if (!(action == "read" || action == "write" || action == "delete" || action == "create"))
            {
                throw new ArgumentOutOfRangeException(nameof(action));
            }

            var sql = 
                @"select distinct ""r"".""_id"", ""r"".""name"", ""r"".""constraint"" from ""core_rule"" ""r"" " +
                @"inner join ""core_meta_entity"" ""m"" on (""r"".""meta_entity"" = ""m"".""_id"") " +
                @"where ""m"".""name""= ? " +
                    @" and (""r"".""on_" + action + @""" = '1') and ((""r"".""global"" = '1') or (""r"".""_id"" in " +
                    @"(select ""rr"".""rule"" from ""core_rule_role_rel"" ""rr"" " +
                        @"inner join ""core_user_role_rel"" ""ur"" on (""rr"".""role"" = ""ur"".""role"") " +
                        @"where ""ur"".""user""=? )))";

            var result = ctx.DataContext.QueryAsDictionary(sql, entityName, ctx.UserSession.UserId);

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
            var userEntity = (UserEntity)ctx.GetResource("core.user");
            dynamic user = userEntity.Browse(ctx.UserSession.UserId);

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
        /// <param name="entityName"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        internal static Criterion[] GetRuleConstraintsCached(IServiceContext scope, string entityName, string action)
        {
            throw new NotImplementedException();
        }
    }
}
