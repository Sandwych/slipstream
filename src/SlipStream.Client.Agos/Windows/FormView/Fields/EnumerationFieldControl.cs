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

using SlipStream.Client.Agos.Models;

namespace SlipStream.Client.Agos.Windows.FormView
{
    public class EnumerationFieldControl : ComboBox, IFieldWidget
    {
        private readonly IDictionary<string, object> metaField;
        private readonly bool isRequired;

        public EnumerationFieldControl(object metaField)
        {
            this.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;

            this.metaField = (IDictionary<string, object>)metaField;
            this.FieldName = (string)this.metaField["name"];
            this.isRequired = (bool)this.metaField["required"];

            if (this.isRequired)
            {
                this.ItemsSource = (IEnumerable)this.metaField["options"];
            }
            else
            {
                var options = new Dictionary<string, string>();
                options.Add(string.Empty, " ");
                dynamic items = this.metaField["options"];
                foreach (dynamic i in items)
                {
                    options.Add(i.Key, i.Value);
                }
                this.ItemsSource = options;
            }

            this.SelectedValuePath = "Key";
            this.DisplayMemberPath = "Value";

            this.IsEnabled = !(bool)this.metaField["readonly"];
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
            if (this.isRequired)
            {
                this.SelectedIndex = -1;
            }
            else
            {
                this.SelectedValue = String.Empty;
            }
        }
    }
}
