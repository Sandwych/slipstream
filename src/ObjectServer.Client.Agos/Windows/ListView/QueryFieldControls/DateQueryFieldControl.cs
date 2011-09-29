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

namespace ObjectServer.Client.Agos.Windows.ListView.QueryFieldControls
{
    public class DateQueryFieldControl : UserControl, IQueryField
    {
        private readonly IDictionary<string, object> metaField;
        private readonly ColumnDefinition col0 =
            new ColumnDefinition() { Width = new GridLength(50, GridUnitType.Star) };
        private readonly ColumnDefinition col1 =
            new ColumnDefinition() { Width = GridLength.Auto };
        private readonly ColumnDefinition col2 =
            new ColumnDefinition() { Width = new GridLength(50, GridUnitType.Star) };

        private readonly DatePicker lowDatePicker = new DatePicker();
        private readonly DatePicker highDatePicker = new DatePicker();

        public DateQueryFieldControl(object metaField)
            : base()
        {
            this.metaField = (IDictionary<string, object>)metaField;
            this.FieldName = (string)this.metaField["name"];

            var grid = new Grid();
            this.Content = grid;
            grid.ColumnDefinitions.Add(col0);
            grid.ColumnDefinitions.Add(col1);
            grid.ColumnDefinitions.Add(col2);

            grid.Children.Add(this.lowDatePicker);
            this.lowDatePicker.SetValue(Grid.ColumnProperty, 0);

            grid.Children.Add(this.highDatePicker);
            this.highDatePicker.SetValue(Grid.ColumnProperty, 2);

            var label = new Label();
            grid.Children.Add(label);
            label.Content = "至";
            label.Margin = new Thickness(3, 0, 3, 0);
            label.SetValue(Grid.ColumnProperty, 1);
        }

        public QueryConstraint[] GetConstraints()
        {
            System.Diagnostics.Debug.Assert(!this.IsEmpty);

            var constraints = new List<QueryConstraint>(2);
            if (this.highDatePicker.SelectedDate != null)
            {
                constraints.Add(
                    new QueryConstraint(this.FieldName, "<=", this.highDatePicker.SelectedDate.Value));
            }

            if (this.lowDatePicker.SelectedDate != null)
            {
                constraints.Add(
                    new QueryConstraint(this.FieldName, ">=", this.lowDatePicker.SelectedDate.Value));
            }

            return constraints.ToArray();
        }

        public void Empty()
        {
            this.lowDatePicker.SelectedDate = null;
            this.highDatePicker.SelectedDate = null;
        }

        public bool IsEmpty
        {
            get
            {
                return this.lowDatePicker.SelectedDate == null && this.highDatePicker.SelectedDate == null;
            }
        }

        public string FieldName { get; private set; }
    }
}
