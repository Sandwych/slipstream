using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ObjectServer.Model;

namespace ObjectServer.Core
{
    /// <summary>
    /// 模型访问控制列表
    /// </summary>
    [Resource]
    public sealed class ModelAccessModel : AbstractTableModel
    {

        public ModelAccessModel()
            : base("core.model_access")
        {
            Fields.ManyToOne("group", "core.group").Required().SetLabel("User Group");
            Fields.ManyToOne("model", "core.model").Required().SetLabel("Model");
            Fields.Chars("description").SetLabel("Description");
            Fields.Boolean("allow_create").SetLabel("Allow Creation")
                .Required().DefaultValueGetter(s => true);
            Fields.Boolean("allow_read").SetLabel("Allow Reading")
                .Required().DefaultValueGetter(s => true);
            Fields.Boolean("allow_write").SetLabel("Allow Writing")
                .Required().DefaultValueGetter(s => true);
            Fields.Boolean("allow_delete").SetLabel("Allrow Deletion")
                .Required().DefaultValueGetter(s => true);
        }


    }
}
