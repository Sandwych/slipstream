using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SlipStream.Model;

namespace SlipStream.Core
{

    [Resource]
    public sealed class MenuModel : AbstractSqlModel
    {

        public MenuModel()
            : base("core.menu")
        {
            this.Hierarchy = true;

            Fields.Chars("name").WithLabel("Name").WithRequired();
            Fields.Integer("ordinal").WithLabel("Ordinal Number")
                .WithRequired().WithDefaultValueGetter(arg => 0);
            Fields.ManyToOne("parent", "core.menu").WithLabel("Parent Menu").WithNotRequired();
            Fields.Chars("icon").WithLabel("Icon Name").WithNotRequired();
            Fields.Boolean("active").WithLabel("Active").WithRequired().WithDefaultValueGetter(arg => true);
            Fields.Reference("action").WithLabel("Action").WithNotRequired().SetOptions(
                   new Dictionary<string, string>()
                {
                    { "core.action_window", "Window Action" },
                    { "core.action_wizard", "Wizard Action" },
                });
        }

    }
}
