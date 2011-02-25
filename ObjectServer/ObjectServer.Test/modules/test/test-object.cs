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
            : base("test.child")
        {
            Fields.Chars("name").SetLabel("Name").SetRequired().SetSize(64);
            Fields.ManyToOne("master", "test.master").SetLabel("Master");
        }
    }


    //主表
    [ServiceObject]
    public sealed class MasterObject : TableModel
    {
        public MasterObject()
            : base("test.master")
        {
            Fields.Chars("name").SetLabel("Name").SetSize(64);
            Fields.OneToMany("children", "test.child", "master").SetLabel("Children");
        }
    }


}
