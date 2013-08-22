using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

using SlipStream.Model;

namespace SlipStream.Test
{

    //////////////////// 多层次分类的表 ///////////////////
    [Resource]
    public sealed class CategoryModel : AbstractSqlModel
    {
        public CategoryModel()
            : base("test.category")
        {
            IsVersioned = false;
            Hierarchy = true;
            Fields.Chars("name").SetLabel("Name").NotRequired().SetSize(64);
        }
    }

}
