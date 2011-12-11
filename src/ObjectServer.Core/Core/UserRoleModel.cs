using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Globalization;

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

        public override void Initialize(IServiceContext ctx, bool update)
        {
            base.Initialize(ctx, update);

            var tableCtx = ctx.DBContext.CreateTableContext(this.TableName);

            if (update && !tableCtx.ConstraintExists(ctx.DBContext, UniqueConstraintName))
            {
                var userCol = DataProvider.Dialect.QuoteForColumnName("user");
                var roleCol = DataProvider.Dialect.QuoteForColumnName("role");
                var sql = string.Format(CultureInfo.InvariantCulture,
                    "UNIQUE({0}, {1})", userCol, roleCol);

                tableCtx.AddConstraint(ctx.DBContext, UniqueConstraintName, sql);
            }
        }
    }
}
