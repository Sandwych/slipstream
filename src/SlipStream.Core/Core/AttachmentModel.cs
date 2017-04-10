using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SlipStream.Model;

namespace SlipStream.Core
{
    [Resource]
    public sealed class AttachmentModel : AbstractSqlModel
    {

        public AttachmentModel()
            : base("core.attachment")
        {
            
            Fields.Chars("name").WithLabel("Name");
            Fields.Chars("res_name").WithLabel("Related Resource");
            Fields.BigInteger("res_id").WithLabel("Related Resource ID.");
            Fields.Text("description").WithLabel("Description");
            Fields.Chars("link").WithLabel("Link");
            Fields.Binary("content").WithLabel("Content");
            Fields.ManyToOne("organization", "core.organization").WithLabel("Organization");
        }


    }
}
