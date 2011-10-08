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

namespace ObjectServer.Core
{
    [Resource]
    public sealed class RuleModel : AbstractSqlModel
    {
        private static readonly Criterion[][] EmptyConstraints = new Criterion[][] { };
        //为了让 VS 复制 IronRuby.Libraries.dll 
        private static readonly Type _rubyHashOpsType = typeof(IronRuby.Builtins.HashOps);

        private static ScriptRuntime s_runtime = Ruby.CreateRuntime();
        private static ScriptEngine s_engine = Ruby.GetEngine(s_runtime);

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
        /// </summary>
        /// <param name="model"></param>
        /// <param name="scope"></param>
        /// <param name="modelName"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        internal static IList<Criterion[]> GetRuleConstraints(ITransactionContext scope, string modelName, string action)
        {
            Debug.Assert(scope != null);
            Debug.Assert(!string.IsNullOrEmpty(modelName));
            Debug.Assert(!string.IsNullOrEmpty(action));
            Debug.Assert(action == "read" || action == "write" || action == "delete" || action == "create");

            //TODO 缓存
            var sql = new SqlString(
                "SELECT DISTINCT r._id, r.name, r.constraint FROM core_rule r ",
                "INNER JOIN core_model m ON (r.model=m._id) ",
                "WHERE m.name=", Parameter.Placeholder,
                    " AND r.on_", action, " AND (r.global OR (r._id IN ",
                    "(SELECT rr.rule FROM core_rule_role_rel rr ",
                        "INNER JOIN core_user_role_rel ur ON (rr.role = ur.role) ",
                        "WHERE ur.user =", Parameter.Placeholder, " )))");

            var result = scope.DBContext.QueryAsDictionary(
                sql, modelName, scope.Session.UserId);

            if (result.Length > 0)
            {
                return ConvertConstraints(scope, result);
            }
            else
            {
                return EmptyConstraints;
            }
        }

        private static IList<Criterion[]> ConvertConstraints(
            ITransactionContext scope, Dictionary<string, object>[] result)
        {
            var scriptScope = CreateScriptScope(scope);

            var constraints = new List<Criterion[]>();
            var cr = new List<Criterion>(4);
            foreach (var row in result)
            {
                cr.Clear();
                var constraintExp = (string)row["constraint"];
                var scriptSource = s_engine.CreateScriptSourceFromString(
                    constraintExp, SourceCodeKind.Expression);
                var compiledCode = scriptSource.Compile();
                var dynObj = compiledCode.Execute(scriptScope);

                foreach (dynamic d in dynObj)
                {
                    var c = new Criterion(
                        (string)d[0], (string)d[1], d[2]);
                    cr.Add(c);
                }
                constraints.Add(cr.ToArray());
            }
            return constraints;
        }

        private static ScriptScope CreateScriptScope(ITransactionContext scope)
        {
            var userModel = (UserModel)scope.GetResource("core.user");
            dynamic user = userModel.Browse(scope, scope.Session.UserId);

            var scriptScope = s_engine.CreateScope();
            scriptScope.SetVariable("user", user);
            return scriptScope;
        }

        /// <summary>
        /// 获取指定模型方法的访问规则
        /// 此方法同 GetRuleDomain，但是经过了缓存
        /// </summary>
        /// <param name="scope"></param>
        /// <param name="modelName"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        internal static Criterion[] GetRuleConstraintsCached(ITransactionContext scope, string modelName, string action)
        {
            throw new NotImplementedException();
        }
    }
}
