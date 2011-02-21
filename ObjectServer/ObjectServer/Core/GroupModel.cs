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
    public sealed class GroupModel : TableModel
    {

        public GroupModel()
        {
            this.Name = "core.group";

            this.CharsField("name", "Name", 128, true, null, null);

            this.ManyToManyField(
                "users", "core.user_group", "gid", "uid", "Users", true, null, null);
        }

    }
}
