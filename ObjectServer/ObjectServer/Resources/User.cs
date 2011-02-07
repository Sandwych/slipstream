using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.Resources
{
    public sealed class User : Model.ModelBase
    {

        public User()
        {
            this.Name = "Core.User";
            this.TableName = "core_user";
        }

    }
}
