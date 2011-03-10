using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using ObjectServer.Model;
using ObjectServer.Utility;
using ObjectServer.Backend;

namespace ObjectServer.Core
{
    [Resource]
    public sealed class UserGroupModel : TableModel
    {

        public UserGroupModel()
            : base("core.user_group")
        {
            this.TableName = "core_user_group_rel";

            Fields.ManyToOne("uid", "core.user").SetLabel("User").Required();
            Fields.ManyToOne("gid", "core.group").SetLabel("Group").Required();

        }
    }
}
