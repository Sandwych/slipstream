using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ObjectServer.Data;
using ObjectServer.Model;

namespace ObjectServer.Core
{
    [Resource]
    public sealed class RuleRoleModel : AbstractSqlModel
    {
        private const string UniqueConstraintName = "unique_rule_role_rel";

        public RuleRoleModel()
            : base("core.rule_role")
        {
            this.TableName = "core_rule_role_rel";

            Fields.ManyToOne("role", "core.role").SetLabel("Role")
                .Required().OnDelete(OnDeleteAction.Cascade);
            Fields.ManyToOne("rule", "core.rule").SetLabel("Rule")
                .Required().OnDelete(OnDeleteAction.Cascade);
        }

        public override void Initialize(ITransactionContext tc, bool update)
        {
            if (tc == null)
            {
                throw new ArgumentNullException("tc");
            }

            base.Initialize(tc, update);

            var tableCtx = tc.DBContext.CreateTableContext(this.TableName);

            if (update && !tableCtx.ConstraintExists(tc.DBContext, UniqueConstraintName))
            {
                tableCtx.AddConstraint(tc.DBContext, UniqueConstraintName, "UNIQUE(\"role\", \"rule\")");
            }
        }
    }
}
