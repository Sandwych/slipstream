using System;
using System.Collections;
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
    public class EnumerationFieldControl : ComboBox, IFieldWidget
    {
        private readonly IDictionary<string, object> metaField;

        public EnumerationFieldControl(object metaField)
        {

            this.VerticalContentAlignment = System.Windows.VerticalAlignment.Center;
            this.VerticalAlignment = System.Windows.VerticalAlignment.Center;

            this.metaField = (IDictionary<string, object>)metaField;
            this.FieldName = (string)this.metaField["name"];

            var options = (IEnumerable)this.metaField["options"];
            this.SelectedValuePath = "Key";
            this.DisplayMemberPath = "Value";
            this.ItemsSource = options;
        }

        public string FieldName { get; private set; }

        public object Value
        {
            get
            {
                return this.SelectedValue;
            }
            set
            {
                var tuple = (object[])value;
                this.SelectedValue = (string)tuple[0];
            }
        }

        public void Empty()
        {
            this.SelectedValue = null;
        }
    }
}
