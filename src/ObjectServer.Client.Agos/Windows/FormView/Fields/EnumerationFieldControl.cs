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
    public class EnumerationFieldControl : TextBox, IFieldWidget
    {
        public EnumerationFieldControl(string fieldName)
        {
            this.DefaultStyleKey = typeof(TextBox);

            this.FieldName = fieldName;
            this.VerticalContentAlignment = System.Windows.VerticalAlignment.Center;
            this.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            this.Margin = new Thickness(5, 2, 5, 2);
        }

        public string FieldName { get; private set; }

        public object Value
        {
            get
            {
                return this.Text;
            }
            set
            {
                var tuple = (object[])value;
                this.Text = (string)tuple[0];
            }
        }

        public void Empty()
        {
            this.Text = String.Empty;
        }
    }
}
