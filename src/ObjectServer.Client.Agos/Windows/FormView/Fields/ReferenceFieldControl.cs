using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

using ObjectServer.Client.Agos.Models;

namespace ObjectServer.Client.Agos.Windows.FormView
{
    public sealed class ReferenceFieldControl : UserControl, IFieldWidget
    {
        private readonly ComboBox modelComboBox;
        private readonly TextBox nameTextBox;
        private readonly Button selectButton;

        private readonly IDictionary<string, object> metaField;

        public ReferenceFieldControl(object metaField)
        {
            this.metaField = (IDictionary<string, object>)metaField;
            this.FieldName = (string)this.metaField["name"];

            var layoutRoot = new Grid();
            this.Content = layoutRoot;
            var col1 = new ColumnDefinition() { Width = new GridLength(2, GridUnitType.Star) };
            var col2 = new ColumnDefinition() { Width = new GridLength(3, GridUnitType.Star) };
            var col3 = new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Auto) };
            layoutRoot.ColumnDefinitions.Add(col1);
            layoutRoot.ColumnDefinitions.Add(col2);
            layoutRoot.ColumnDefinitions.Add(col3);

            this.modelComboBox = new ComboBox();
            this.modelComboBox.SetValue(Grid.ColumnProperty, 0);
            this.modelComboBox.VerticalContentAlignment = System.Windows.VerticalAlignment.Center;
            layoutRoot.Children.Add(modelComboBox);

            this.nameTextBox = new TextBox();
            this.nameTextBox.SetValue(Grid.ColumnProperty, 1);
            this.nameTextBox.VerticalContentAlignment = System.Windows.VerticalAlignment.Center;
            layoutRoot.Children.Add(nameTextBox);

            this.selectButton = new Button();
            this.selectButton.SetValue(Grid.ColumnProperty, 2);
            this.selectButton.Content = "...";
            layoutRoot.Children.Add(selectButton);

            //this.VerticalContentAlignment = System.Windows.VerticalAlignment.Center;
            //this.VerticalAlignment = System.Windows.VerticalAlignment.Center;

            var options = (IEnumerable)this.metaField["options"];
            this.modelComboBox.SelectedValuePath = "Key";
            this.modelComboBox.DisplayMemberPath = "Value";
            this.modelComboBox.ItemsSource = options;
        }

        public string FieldName { get; private set; }

        private object[] fieldValue;
        public object Value
        {
            get
            {
                return new object[] { this.fieldValue[0], this.fieldValue[1] };
            }
            set
            {
                var tuple = value as object[];
                if (tuple != null)
                {
                    this.fieldValue = tuple;
                    this.modelComboBox.SelectedValue = tuple[0];
                    this.nameTextBox.Text = (string)tuple[2];
                }
            }
        }

        public void Empty()
        {
            this.nameTextBox.Text = String.Empty;
        }
    }
}
