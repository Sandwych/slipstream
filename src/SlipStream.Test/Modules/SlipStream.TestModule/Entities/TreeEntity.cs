using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

using SlipStream.Entity;

namespace SlipStream.Test
{

    //////////////////// 多层次分类的表 ///////////////////
    [Resource]
    public sealed class CategoryEntity : AbstractSqlEntity
    {
        public CategoryEntity()
            : base("test.category")
        {
            IsVersioned = false;
            Hierarchy = true;
            Fields.Chars("name").WithLabel("Name").WithNotRequired().WithSize(64);
        }
    }

}
