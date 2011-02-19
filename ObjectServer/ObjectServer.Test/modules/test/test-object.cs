using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;

using ObjectServer.Model;

namespace ObjectServer.Test
{
    [ServiceObject]
    public sealed class MasterObject : TableModel
    {
        public MasterObject()
        {
            this.Name = "test.master";
            this.CharsField("name", "Name", 64, true, null, null);
            this.OneToManyField("children", "test.children", "master", "Children", false, null, null);
        }
    }

    [ServiceObject]
    public sealed class ChildObject : TableModel
    {
        public ChildObject()
        {
            this.Name = "test.child";
            this.CharsField("name", "Name", 64, true, null, null);
            this.ManyToOneField("master", "test.master", "Master", true, null, null);
        }
    }


}
