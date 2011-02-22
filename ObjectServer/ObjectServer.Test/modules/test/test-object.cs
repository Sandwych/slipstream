using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;

using ObjectServer.Model;

namespace ObjectServer.Test
{

    //我们故意反转依赖顺序进行声明，看系统能否处理

    //子表
    [ServiceObject]
    public sealed class ChildObject : TableModel
    {
        public ChildObject()
        {
            this.Name = "test.child";
            this.CharsField("name", "Name", 64, true, null, null);
            this.ManyToOneField("master", "test.master", "Master", false, null, null);
        }
    }


    //主表
    [ServiceObject]
    public sealed class MasterObject : TableModel
    {
        public MasterObject()
        {
            this.Name = "test.master";
            this.CharsField("name", "Name", 64, false, null, null);
            this.OneToManyField("children", "test.child", "master", "Children", false, null, null);
        }
    }


}
