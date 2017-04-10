using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using SlipStream.Entity;
using SlipStream.Data;

namespace SlipStream.Core
{

    [Resource]
    public sealed class AuditLogEntity : AbstractSqlEntity
    {
        public const string EntityName = "core.audit_log";

        public AuditLogEntity()
            : base(EntityName)
        {
            Fields.ManyToOne("user", "core.user").WithLabel("User");
            Fields.Boolean("marked").WithLabel("Marked As Read")
                .WithRequired().WithDefaultValueGetter(ctx => false);
            Fields.Chars("resource").WithLabel("Resource Name").WithSize(64).WithRequired();
            Fields.BigInteger("resource_id").WithLabel("Resource ID").WithRequired();
            Fields.Chars("description").WithLabel("Description")
                .WithRequired().WithSize(256);
        }

    }
}
