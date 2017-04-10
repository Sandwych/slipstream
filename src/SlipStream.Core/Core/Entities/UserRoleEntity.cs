using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Globalization;

using SlipStream.Entity;
using SlipStream.Data;

namespace SlipStream.Core
{
    [Resource]
    public sealed class UserRoleEntity : AbstractSqlEntity
    {
        private const string UniqueConstraintName = "unique_core_user_role";

        public UserRoleEntity() : base("core.user_role")
        {
            this.TableName = "core_user_role_rel";

            Fields.ManyToOne("user", "core.user").WithLabel("User").WithRequired();
            Fields.ManyToOne("role", "core.role").WithLabel("Role").WithRequired();

        }

        public override void Initialize(bool update)
        {
            base.Initialize(update);
            var ctx = this.DbDomain.CurrentSession;
            var tableCtx = ctx.DataContext.CreateTableContext(this.TableName);
            if (update && !tableCtx.ConstraintExists(ctx.DataContext, UniqueConstraintName))
            {
                var userCol = this.DbDomain.DataProvider.Dialect.QuoteForColumnName("user");
                var roleCol = this.DbDomain.DataProvider.Dialect.QuoteForColumnName("role");
                var sql = $"UNIQUE({userCol}, {roleCol})";
                tableCtx.AddConstraint(ctx.DataContext, UniqueConstraintName, sql);
            }
        }
    }
}
