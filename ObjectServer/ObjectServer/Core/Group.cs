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
    public sealed class Group : ModelBase
    {

        public Group()
        {
            this.Name = "core.group";
        }

    }
}
