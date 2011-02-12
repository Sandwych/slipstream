using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ObjectServer.Model;

namespace ObjectServer.Core
{

    [ServiceObject]
    public sealed class Module : ModelBase
    {

        public Module()
        {
            this.Name = "core.module";
            this.Automatic = false;


            this.DefineField("name", "Name", "varchar", 128);
            this.DefineField("state", "State", "varchar", 16);
            this.DefineField("description", "Description", "text", 0xffff);
        }

    }
}
