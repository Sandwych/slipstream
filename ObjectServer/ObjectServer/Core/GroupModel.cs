using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ObjectServer.Model;

namespace ObjectServer.Core
{

    /// <summary>
    /// 用户组信息
    /// </summary>
    [ServiceObject]
    public sealed class GroupModel : ModelBase
    {

        public GroupModel()
        {
            this.Name = "core.group";

            /*
            ManyToManyField(
                "users", "core.user", "core_user_group_rel", "group_id", "user_id",
                "Users", false, null);
           */
        }

    }
}
