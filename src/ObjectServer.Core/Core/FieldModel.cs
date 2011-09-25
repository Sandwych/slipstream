using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ObjectServer.Model;

namespace ObjectServer.Core
{
    [Resource]
    public class FieldModel : AbstractTableModel
    {
        public const string ModelName = "core.field";
        public const int FieldNameMax = 64;

        public FieldModel()
            : base(ModelName)
        {
            this.AutoMigration = false;

            Fields.ManyToOne("model", "core.model").SetLabel("Model").Required();
            Fields.Chars("name").SetLabel("Name").SetSize(64).Required();
            Fields.Chars("label").SetLabel("Label").SetSize(256).NotRequired();
            Fields.Boolean("required").SetLabel("Required").Required();
            Fields.Boolean("readonly").SetLabel("Read Only").Required();
            Fields.Chars("relation").SetLabel("Relation").SetSize(256).NotRequired();
            Fields.Chars("type").SetLabel("Type").SetSize(32).Required();
            Fields.Chars("help").SetLabel("Help").SetSize(256).NotRequired();
        }


    }
}
