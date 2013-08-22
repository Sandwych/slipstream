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
            
            Fields.Chars("name").SetLabel("Name");
            Fields.Chars("res_name").SetLabel("Related Resource");
            Fields.BigInteger("res_id").SetLabel("Related Resource ID.");
            Fields.Text("description").SetLabel("Description");
            Fields.Chars("link").SetLabel("Link");
            Fields.Binary("content").SetLabel("Content");
            Fields.ManyToOne("organization", "core.organization").SetLabel("Organization");
        }


    }
}
