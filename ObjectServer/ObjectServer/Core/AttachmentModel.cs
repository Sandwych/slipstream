using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ObjectServer.Model;

namespace ObjectServer.Core
{
    [Resource]
    public sealed class AttachmentModel : TableModel
    {

        public AttachmentModel()
            : base("core.attachment")
        {
            
            /*
  description text, -- Description
  url character varying(512), -- Url
  res_model character varying(64), -- Resource Object
  company_id integer, -- Company
  res_name character varying(128), -- Resource Name
  datas_fname character varying(256), -- Filename
  "type" character varying(16) NOT NULL, -- Type
  res_id integer, -- Resource ID
  datas bytea, -- Data
            */
            Fields.Chars("name").SetLabel("Name");
            Fields.Text("description").SetLabel("Description");
            Fields.Chars("link").SetLabel("Link");

            Fields.Binary("content").SetLabel("Content");
        }


    }
}
