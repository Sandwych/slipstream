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
            Fields.ManyToOne("user", "core.user").SetLabel("User");
            Fields.Boolean("marked").SetLabel("Marked As Read")
                .Required().SetDefaultValueGetter(ctx => false);
            Fields.Chars("resource").SetLabel("Resource Name").SetSize(64).Required();
            Fields.BigInteger("resource_id").SetLabel("Resource ID").Required();
            Fields.Chars("description").SetLabel("Description")
                .Required().SetSize(256);
        }

    }
}
