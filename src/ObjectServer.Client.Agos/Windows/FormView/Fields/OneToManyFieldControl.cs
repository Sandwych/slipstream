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
    public class OneToManyFieldControl : Grid, IFieldWidget
    {
        private readonly IDictionary<string, object> metaField;
        private Border border;

        public OneToManyFieldControl(object metaField)
        {
            this.metaField = (IDictionary<string, object>)metaField;
            this.FieldName = (string)this.metaField["name"];

            this.MinHeight = 120;

            this.RowDefinitions.Add(
                new RowDefinition()
                {
                    Height = new GridLength(100, GridUnitType.Star)
                });

            this.border = new Border();
            this.Children.Add(this.border);
            this.border.SetValue(Grid.RowProperty, 0);
            this.border.BorderThickness = new Thickness(1);
            this.border.BorderBrush = new SolidColorBrush(Color.FromArgb(0xff, 0x99, 0x99, 0x99));

            this.Margin = new Thickness(5, 2, 5, 2);
        }

        public string FieldName { get; private set; }

        public object Value
        {
            get
            {
                return null;
            }
            set
            {
            }
        }

        public void Empty()
        {
        }
    }
}
