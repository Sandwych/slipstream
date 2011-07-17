using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ObjectServer.Model;

namespace ObjectServer.Core
{
    [Resource]
    public sealed class RuleGroupModel : AbstractTableModel
    {
        private const string UniqueConstraintName = "unique_rule_group_rel";

        public RuleGroupModel()
            : base("core.rule_group")
        {
            this.TableName = "core_rule_group_rel";

            Fields.ManyToOne("rid", "core.rule").SetLabel("Rule").Required();
            Fields.ManyToOne("gid", "core.group").SetLabel("Group").Required();

        }

        public override void Load(IDBProfile db)
        {
            base.Load(db);

            var tableCtx = db.Connection.CreateTableContext(this.TableName);

            if (!tableCtx.ConstraintExists(db.Connection, UniqueConstraintName))
            {
                tableCtx.AddConstraint(db.Connection, UniqueConstraintName, "UNIQUE(gid, rid)");
            }
        }
    }
}
