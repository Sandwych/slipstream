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

using SlipStream.Client.Agos.Models;

namespace SlipStream.Client.Agos.Windows.FormView
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
            this.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
            this.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            this.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;

            this.IsReadOnly = (bool)this.metaField["readonly"];
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
