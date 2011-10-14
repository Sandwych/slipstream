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
    [TemplatePart(Name = IntegerQueryFieldControl.ElementLowUpDown, Type = typeof(NullableInt32UpDown))]
    [TemplatePart(Name = IntegerQueryFieldControl.ElementHighUpDown, Type = typeof(NullableInt32UpDown))]
    public class IntegerQueryFieldControl : Control, IQueryField
    {
        public const string ElementRoot = "Root";
        public const string ElementLowUpDown = "LowUpDown";
        public const string ElementHighUpDown = "HighUpDown";

        private readonly IDictionary<string, object> metaField;

        private FrameworkElement root;
        private NullableInt32UpDown lowUpdown;
        private NullableInt32UpDown highUpdown;

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
            this.lowUpdown = this.GetTemplateChild(ElementLowUpDown) as NullableInt32UpDown;
            this.highUpdown = this.GetTemplateChild(ElementHighUpDown) as NullableInt32UpDown;
        }

        public QueryConstraint[] GetConstraints()
        {
            System.Diagnostics.Debug.Assert(!this.IsEmpty);

            var constraints = new List<QueryConstraint>(2);
            if (this.highUpdown.Value != null)
            {
                constraints.Add(new QueryConstraint(this.FieldName, "<=", (int)this.highUpdown.Value));
            }

            if (this.lowUpdown.Value != null)
            {
                constraints.Add(new QueryConstraint(this.FieldName, ">=", (int)this.lowUpdown.Value));
            }

            return constraints.ToArray();
        }

        public void Empty()
        {
            if (this.lowUpdown != null)
            {
                this.lowUpdown.Value = null;
            }
            if (this.highUpdown != null)
            {
                this.highUpdown.Value = null;
            }
        }

        public bool IsEmpty
        {
            get
            {
                return this.lowUpdown.Value == null && this.highUpdown.Value == null;
            }
        }

        public string FieldName { get; private set; }
    }
}
