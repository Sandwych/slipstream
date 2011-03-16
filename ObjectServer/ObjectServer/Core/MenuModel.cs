using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ObjectServer.Model;

namespace ObjectServer.Core
{

    [Resource]
    public sealed class MenuModel : AbstractTableModel
    {

        public MenuModel()
            : base("core.menu")
        {
            this.Hierarchy = false; //这里没必要当作树形结构处理，因为处理菜单树是客户端的事情

            Fields.ManyToOne("parent", "core.menu").SetLabel("Parent Menu").NotRequired();
            Fields.Chars("name").SetLabel("Name").Required();
            Fields.Integer("ordinal").SetLabel("Ordinal Number")
                .Required().DefaultValueGetter(arg => 0);
            Fields.Boolean("active").SetLabel("Active").Required().DefaultValueGetter(arg => true);
        }

    }
}
