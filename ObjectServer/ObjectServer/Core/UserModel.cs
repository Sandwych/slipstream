using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ObjectServer.Model;

namespace ObjectServer.Core
{

    [ServiceObject]
    public sealed class UserModel : ModelBase
    {

        public UserModel()
        {
            this.Name = "core.user";


            /*
            ManyToManyField(
                "groups", "core.group", "core_user_group_rel", "user_id", "group_id",
                "User Groups", false, null);
             */
        }

    }
}
