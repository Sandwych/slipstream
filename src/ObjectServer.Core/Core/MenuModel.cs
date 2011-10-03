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
            this.Hierarchy = true;

            Fields.ManyToOne("parent", "core.menu").SetLabel("Parent Menu").NotRequired();
            Fields.Chars("name").SetLabel("Name").Required();
            Fields.Integer("ordinal").SetLabel("Ordinal Number")
                .Required().SetDefaultValueGetter(arg => 0);
            Fields.Boolean("active").SetLabel("Active").Required().SetDefaultValueGetter(arg => true);
            Fields.Reference("action").SetLabel("Action").NotRequired().SetOptions(
                   new Dictionary<string, string>()
                {
                    { "core.action_window", "Window Action" },
                    { "core.action_wizard", "Wizard Action" },
                });
        }

    }
}
