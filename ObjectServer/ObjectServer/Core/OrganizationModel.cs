using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ObjectServer.Model;

namespace ObjectServer.Core
{
    [Resource]
    public sealed class OrganizationModel : TableModel
    {

        public OrganizationModel()
            : base("core.organization")
        {
            Fields.Chars("code").SetLabel("Code").SetSize(64).Required();
            Fields.Chars("name").SetLabel("Name").Required();
        }


    }
}
