using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

using ObjectServer.Model;

namespace ObjectServer.Test
{

    //////////////////// 多层次分类的表 ///////////////////
    [Resource]
    public sealed class  CategoryModel : AbstractSqlModel
    {
        public CategoryModel()
            : base("test.category")
        {
            this.Hierarchy = true;
            Fields.Chars("name").SetLabel("Name").NotRequired().SetSize(64);
        }
    }

}
