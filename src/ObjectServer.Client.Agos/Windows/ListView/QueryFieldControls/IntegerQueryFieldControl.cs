using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;

using ObjectServer.Client.Agos.Models;
using ObjectServer.Client.Agos.Controls;

namespace ObjectServer.Client.Agos.Windows.TreeView.QueryFieldControls
{
    [TemplatePart(Name = IntegerQueryFieldControl.ElementRoot, Type = typeof(FrameworkElement))]
    [TemplatePart(Name = IntegerQueryFieldControl.ElementLowUpDown, Type = typeof(NumericUpDown))]
    [TemplatePart(Name = IntegerQueryFieldControl.ElementHighUpDown, Type = typeof(NumericUpDown))]
    public class IntegerQueryFieldControl : Control, IQueryField
    {
        public const string ElementRoot = "Root";
        public const string ElementLowUpDown = "LowUpDown";
        public const string ElementHighUpDown = "HighUpDown";

        private readonly IDictionary<string, object> metaField;

        private FrameworkElement root;
        private NumericUpDown lowUpdown;
        private NumericUpDown highUpdown;

        public IntegerQueryFieldControl(object metaField)
            : base()
        {
            this.metaField = (IDictionary<string, object>)metaField;
            this.FieldName = (string)this.metaField["name"];
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this.root = this.GetTemplateChild(ElementRoot) as FrameworkElement;
            this.lowUpdown = this.GetTemplateChild(ElementLowUpDown) as NumericUpDown;
            this.highUpdown = this.GetTemplateChild(ElementHighUpDown) as NumericUpDown;
        }

        public QueryConstraint[] GetConstraints()
        {
            System.Diagnostics.Debug.Assert(!this.IsEmpty);

            var constraints = new List<QueryConstraint>(2);
            if (!double.IsNaN(this.highUpdown.Value))
            {
                constraints.Add(new QueryConstraint(this.FieldName, "<=", (int)this.highUpdown.Value));
            }

            if (!double.IsNaN(this.lowUpdown.Value))
            {
                constraints.Add(new QueryConstraint(this.FieldName, ">=", (int)this.lowUpdown.Value));
            }

            return constraints.ToArray();
        }

        public void Empty()
        {
            if (this.lowUpdown != null)
            {
                this.lowUpdown.Value = double.NaN;
            }
            if (this.highUpdown != null)
            {
                this.highUpdown.Value = double.NaN;
            }
        }

        public bool IsEmpty
        {
            get
            {
                return double.IsNaN(this.lowUpdown.Value) && double.IsNaN(this.highUpdown.Value);
            }
        }

        public string FieldName { get; private set; }
    }
}
