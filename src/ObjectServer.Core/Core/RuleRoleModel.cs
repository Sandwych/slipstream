using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ObjectServer.Data;
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

        public override void Initialize(IDBContext db, bool update)
        {
            if (db == null)
            {
                throw new ArgumentNullException("db");
            }

            base.Initialize(db, update);

            var tableCtx = db.CreateTableContext(this.TableName);

            if (update && !tableCtx.ConstraintExists(db, UniqueConstraintName))
            {
                tableCtx.AddConstraint(db, UniqueConstraintName, "UNIQUE(\"role\", \"rule\")");
            }
        }
    }
}
