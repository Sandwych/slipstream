using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ObjectServer.Model;

namespace ObjectServer.Core
{

    [Resource]
    public sealed class ViewModel : AbstractTableModel
    {

        public ViewModel()
            : base("core.view")
        {

            Fields.Chars("name").SetLabel("Name").SetSize(64).Required();
            Fields.Chars("model").SetLabel("Model").SetSize(128).Required();
            Fields.Enumeration("kind",
                new Dictionary<string, string>() { 
                    { "form", "Form View" }, 
                    { "list", "List View" },
                    { "tree", "Tree View" },
                })
                .SetLabel("View Kind").Required();
            Fields.Text("layout").SetLabel("Layout");
        }

    }
}
