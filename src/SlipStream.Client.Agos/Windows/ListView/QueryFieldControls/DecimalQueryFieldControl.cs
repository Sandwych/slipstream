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

using SlipStream.Client.Agos.Models;
using SlipStream.Client.Agos.Controls;

namespace SlipStream.Client.Agos.Windows.TreeView.QueryFieldControls
{
    [TemplatePart(Name = DecimalQueryFieldControl.ElementRoot, Type = typeof(FrameworkElement))]
    [TemplatePart(Name = DecimalQueryFieldControl.ElementLowUpDown, Type = typeof(NullableDecimalUpDown))]
    [TemplatePart(Name = DecimalQueryFieldControl.ElementHighUpDown, Type = typeof(NullableDecimalUpDown))]
    public class DecimalQueryFieldControl : Control, IQueryField
    {
        public const string ElementRoot = "Root";
        public const string ElementLowUpDown = "LowUpDown";
        public const string ElementHighUpDown = "HighUpDown";

        private readonly IDictionary<string, object> metaField;

        private FrameworkElement root;
        private NullableDecimalUpDown lowUpdown;
        private NullableDecimalUpDown highUpdown;

        public DecimalQueryFieldControl(object metaField)
            : base()
        {
            this.metaField = (IDictionary<string, object>)metaField;
            this.FieldName = (string)this.metaField["name"];
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this.root = this.GetTemplateChild(ElementRoot) as FrameworkElement;
            this.lowUpdown = this.GetTemplateChild(ElementLowUpDown) as NullableDecimalUpDown;
            this.highUpdown = this.GetTemplateChild(ElementHighUpDown) as NullableDecimalUpDown;
        }

        public QueryConstraint[] GetConstraints()
        {
            System.Diagnostics.Debug.Assert(!this.IsEmpty);

            var constraints = new List<QueryConstraint>(2);
            if (this.highUpdown.Value != null)
            {
                constraints.Add(new QueryConstraint(this.FieldName, "<=", (decimal)this.highUpdown.Value));
            }

            if (this.lowUpdown.Value != null)
            {
                constraints.Add(new QueryConstraint(this.FieldName, ">=", (decimal)this.lowUpdown.Value));
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
