using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using SlipStream.Model;
using SlipStream.Data;

namespace SlipStream.Core
{

    [Resource]
    public sealed class AuditLogModel : AbstractSqlModel
    {
        public const string ModelName = "core.audit_log";

        public AuditLogModel()
            : base(ModelName)
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
