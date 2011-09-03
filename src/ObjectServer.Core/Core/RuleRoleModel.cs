using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ObjectServer.Model;

namespace ObjectServer.Core
{
    [Resource]
    public sealed class RuleRoleModel : AbstractTableModel
    {
        private const string UniqueConstraintName = "unique_rule_role_rel";

        public RuleRoleModel()
            : base("core.rule_role")
        {
            this.TableName = "core_rule_role_rel";

            Fields.ManyToOne("role", "core.role").SetLabel("Role").Required();
            Fields.ManyToOne("rule", "core.rule").SetLabel("Rule").Required();
        }

        public override void Initialize(IDBProfile db, bool update)
        {
            base.Initialize(db, update);

            var tableCtx = db.DBContext.CreateTableContext(this.TableName);

            if (update && !tableCtx.ConstraintExists(db.DBContext, UniqueConstraintName))
            {
                tableCtx.AddConstraint(db.DBContext, UniqueConstraintName, "UNIQUE(\"role\", \"rule\")");
            }
        }
    }
}
