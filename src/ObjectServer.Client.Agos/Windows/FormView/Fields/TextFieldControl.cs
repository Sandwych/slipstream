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
    public class TextFieldControl : TextBox, IFieldWidget
    {
        private readonly IDictionary<string, object> metaField;

        public TextFieldControl(object metaField)
        {
            this.DefaultStyleKey = typeof(TextBox);

            this.metaField = (IDictionary<string, object>)metaField;
            this.FieldName = (string)this.metaField["name"];

            this.AcceptsReturn = true;
            this.VerticalContentAlignment = System.Windows.VerticalAlignment.Top;
            this.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            this.MinHeight = 120;
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
                this.Text = value as string ?? string.Empty;
            }
        }

        public void Empty()
        {
            this.Text = String.Empty;
        }
    }
}
