using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

using Malt.Layout.Widgets;

using ObjectServer.Client.Agos.Models;

namespace ObjectServer.Client.Agos.Windows.FormView
{
    public class DateTimeFieldControl : Grid, IFieldWidget
    {
        private DatePicker datePicker;
        private TimeUpDown timeUpDown;

        public DateTimeFieldControl(string fieldName)
        {
            var col1 = new ColumnDefinition() { Width = GridLength.Auto }; //new GridLength(50, GridUnitType.Star) };
            var col2 = new ColumnDefinition() { Width = GridLength.Auto }; //new GridLength(50, GridUnitType.Star) };

            this.ColumnDefinitions.Add(col1);
            this.ColumnDefinitions.Add(col2);

            this.datePicker = new DatePicker();
            this.datePicker.SetValue(Grid.ColumnProperty, 0);
            this.datePicker.VerticalContentAlignment = System.Windows.VerticalAlignment.Center;
            this.datePicker.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            this.Children.Add(datePicker);

            this.timeUpDown = new TimeUpDown();
            this.timeUpDown.SetValue(Grid.ColumnProperty, 1);
            this.timeUpDown.VerticalContentAlignment = System.Windows.VerticalAlignment.Center;
            this.timeUpDown.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            this.Children.Add(timeUpDown);

            this.FieldName = fieldName;
            this.Margin = new Thickness(5, 2, 5, 2);
        }

        public string FieldName { get; private set; }

        public object Value
        {
            get
            {
                if (this.datePicker.SelectedDate != null && this.timeUpDown.Value != null)
                {
                    var date = this.datePicker.SelectedDate.Value;
                    var time = this.timeUpDown.Value.Value;
                    var dt = new DateTime(
                        date.Year, date.Month, date.Day,
                        time.Hour, time.Minute, time.Second, time.Millisecond);
                    return dt;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                this.datePicker.SelectedDate = (DateTime?)value;
                this.timeUpDown.Value = (DateTime?)value;
            }
        }

        public void Empty()
        {
            this.datePicker.SelectedDate = null;
            this.timeUpDown.Value = null;
        }
    }
}
