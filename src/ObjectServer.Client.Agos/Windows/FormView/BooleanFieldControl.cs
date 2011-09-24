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
    public class BooleanFieldControl : CheckBox, IFieldWidget
    {
        public BooleanFieldControl(string fieldName)
        {
            this.DefaultStyleKey = typeof(CheckBox);

            this.FieldName = fieldName;

            this.VerticalContentAlignment = System.Windows.VerticalAlignment.Center;
            this.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            this.Margin = new Thickness(10, 2, 10, 2);
        }

        public string FieldName { get; private set; }

        public object Value
        {
            get
            {
                return this.IsChecked;
            }
            set
            {
                this.IsChecked = (bool)value;
            }
        }

        public void Empty()
        {
            this.IsChecked = false;
        }
    }
}
