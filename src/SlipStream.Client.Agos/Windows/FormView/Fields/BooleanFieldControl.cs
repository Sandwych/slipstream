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
    public sealed class BooleanFieldControl : CheckBox, IFieldWidget
    {
        private readonly IDictionary<string, object> metaField;
        private readonly bool isRequired;

        public BooleanFieldControl(object metaField)
        {
            this.DefaultStyleKey = typeof(CheckBox);

            this.metaField = (IDictionary<string, object>)metaField;
            this.FieldName = (string)this.metaField["name"];

            this.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            this.VerticalContentAlignment = System.Windows.VerticalAlignment.Center;
            this.isRequired = (bool)this.metaField["required"];
            this.IsThreeState = !this.isRequired;
            this.IsEnabled = !(bool)this.metaField["readonly"];
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
            if (this.IsThreeState)
            {
                this.IsChecked = null;
            }
            else
            {
                this.IsChecked = false;
            }
        }
    }
}
