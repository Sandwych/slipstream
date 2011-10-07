using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using ObjectServer.Model;
using ObjectServer.Utility;
using ObjectServer.Data;

namespace ObjectServer.Core
{
    [Resource]
    public sealed class UserRoleModel : AbstractSqlModel
    {
        private const string UniqueConstraintName = "unique_core_user_role";

        public UserRoleModel()
            : base("core.user_role")
        {
            this.TableName = "core_user_role_rel";

            Fields.ManyToOne("user", "core.user").SetLabel("User").Required();
            Fields.ManyToOne("role", "core.role").SetLabel("Role").Required();

        }

        public override void Initialize(ITransactionContext tc, bool update)
        {
            base.Initialize(tc, update);

            var tableCtx = tc.DBContext.CreateTableContext(this.TableName);

            if (update && !tableCtx.ConstraintExists(tc.DBContext, UniqueConstraintName))
            {
                tableCtx.AddConstraint(tc.DBContext, UniqueConstraintName, "UNIQUE(\"user\", \"role\")");
            }
        }
    }
}
