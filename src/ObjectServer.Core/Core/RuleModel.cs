using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Data;
using System.Linq.Expressions;

using System.Reflection;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using IronRuby;

using ObjectServer.Model;

namespace ObjectServer.Core
{
    [Resource]
    public sealed class RuleModel : AbstractTableModel
    {
        //为了让 VS 复制 IronRuby.Libraries
        private static readonly Type _rubyHashOpsType = typeof(IronRuby.Builtins.HashOps);
        private static ScriptRuntime s_runtime = Ruby.CreateRuntime();
        private static ScriptEngine s_engine = Ruby.GetEngine(s_runtime);

        public RuleModel()
            : base("core.rule")
        {
            Fields.Chars("name").SetLabel("Name").Required();
            Fields.ManyToOne("model", "core.model").Required().SetLabel("Model");
            Fields.Boolean("global").SetLabel("Global")
                .Required().DefaultValueGetter(s => true);
            Fields.Chars("field").SetLabel("Field").Required().SetSize(FieldModel.FieldNameMax);
            Fields.Enumeration("operator",
                new Dictionary<string, string>() { 
                    { "=", "=" }, 
                    { "<>", "<>" }, 
                    { "<=", "<=" }, 
                    { "=>", "=>" }, 
                    { "in", "in" }, 
                    { ">", ">" }, 
                    { "<", "<" }, 
                    { "childof", "Child Of" } })
                .Required().SetLabel("Operator");
            Fields.Chars("operand").SetLabel("Operand").Required().SetSize(128);
            Fields.Boolean("on_create").SetLabel("Apply for Creation")
               .Required().DefaultValueGetter(s => true);
            Fields.Boolean("on_read").SetLabel("Apply for Reading")
                .Required().DefaultValueGetter(s => true);
            Fields.Boolean("on_write").SetLabel("Apply for Writing")
                .Required().DefaultValueGetter(s => true);
            Fields.Boolean("on_delete").SetLabel("Apply for Deleting")
                .Required().DefaultValueGetter(s => true);
            Fields.ManyToMany("groups", "core.user_group", "rid", "gid").SetLabel("Groups");

        }

        /// <summary>
        /// 获取指定模型方法的访问规则
        /// </summary>
        /// <param name="model"></param>
        /// <param name="scope"></param>
        /// <param name="modelName"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        internal static DomainExpression[] GetRuleDomain(IServiceScope scope, string modelName, string action)
        {
            Debug.Assert(scope != null);
            Debug.Assert(!string.IsNullOrEmpty(modelName));
            Debug.Assert(!string.IsNullOrEmpty(action));
            Debug.Assert(action == "read" || action == "write" || action == "delete" || action == "create");

            //TODO 缓存

            var sql = @"
SELECT DISTINCT r.id, r.name, r.field, r.operator, r.operand FROM core_rule r
	INNER JOIN core_model m ON (r.model = m.id)
	WHERE m.name = @0 AND r.on_{0}
    AND (r.global OR (r.id IN 
        (SELECT rg.rid  FROM core_rule_group_rel rg 
            INNER JOIN core_user_group_rel ug ON (rg.gid = ug.gid) 
            WHERE ug.uid = @1)))";
            sql = string.Format(sql, action);
            var result = scope.Connection.QueryAsDataTable(
                sql, modelName, scope.Session.UserId);


            var scriptScope = CreateScriptScope(scope);

            var domains = new DomainExpression[result.Rows.Count];
            for (int i = 0; i < result.Rows.Count; i++)
            {
                var row = result.Rows[i];

                var exp = (string)row["operand"];

                var operand = s_engine.Execute(exp, scriptScope);
                var domain = new DomainExpression(
                    (string)row["field"], (string)row["operator"], operand);
                domains[i] = domain;
            }

            return domains;
        }

        private static ScriptScope CreateScriptScope(IServiceScope scope)
        {
            var userModel = (UserModel)scope.GetResource("core.user");
            dynamic user = userModel.Browse(scope, scope.Session.UserId);

            var scriptScope = s_engine.CreateScope();
            scriptScope.SetVariable("user", user);
            scriptScope.SetVariable("now", DateTime.Now);
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
        internal static DomainExpression[] GetRuleDomainCached(IServiceScope scope, string modelName, string action)
        {
            throw new NotImplementedException();
        }
    }
}
