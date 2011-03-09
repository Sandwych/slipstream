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
    [Resource]
    public sealed class GroupModel : TableModel
    {

        public GroupModel()
            : base("core.group")
        {

            Fields.Chars("name").SetLabel("Name").SetSize(128).Required();
            Fields.ManyToMany("users", "core.user_group", "gid", "uid").SetLabel("Users").Required();
        }

    }
}
