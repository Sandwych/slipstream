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
using System.Windows.Media.Animation;
using System.Windows.Shapes;

using Malt.Layout.Widgets;

using ObjectServer.Client.Agos.Models;

namespace ObjectServer.Client.Agos.Windows.FormView
{
    public class ReferenceFieldControl : Grid, IFieldWidget
    {
        private readonly ComboBox modelComboBox;
        private readonly TextBox nameTextBox;
        private readonly Button selectButton;

        private readonly IDictionary<string, object> metaField;

        public ReferenceFieldControl(object metaField)
        {
            this.metaField = (IDictionary<string, object>)metaField;
            this.FieldName = (string)this.metaField["name"];

            var col1 = new ColumnDefinition() { Width = new GridLength(40, GridUnitType.Star) };
            var col2 = new ColumnDefinition() { Width = new GridLength(60, GridUnitType.Star) };
            var col3 = new ColumnDefinition() { Width = GridLength.Auto, };
            this.ColumnDefinitions.Add(col1);
            this.ColumnDefinitions.Add(col2);
            this.ColumnDefinitions.Add(col3);

            this.modelComboBox = new ComboBox();
            this.modelComboBox.SetValue(Grid.ColumnProperty, 0);
            this.modelComboBox.VerticalContentAlignment = System.Windows.VerticalAlignment.Center;
            this.modelComboBox.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            this.Children.Add(modelComboBox);

            this.nameTextBox = new TextBox();
            this.nameTextBox.SetValue(Grid.ColumnProperty, 1);
            this.nameTextBox.VerticalContentAlignment = System.Windows.VerticalAlignment.Center;
            this.nameTextBox.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            this.Children.Add(nameTextBox);

            this.selectButton = new Button();
            this.selectButton.SetValue(Grid.ColumnProperty, 2);
            this.selectButton.Content = "Select";
            this.Children.Add(selectButton);

            //this.VerticalContentAlignment = System.Windows.VerticalAlignment.Center;
            //this.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            this.Margin = new Thickness(5, 2, 5, 2);

            var options = (IEnumerable)this.metaField["options"];
            this.modelComboBox.SelectedValuePath = "Key";
            this.modelComboBox.DisplayMemberPath = "Value";
            this.modelComboBox.ItemsSource = options;
        }

        public string FieldName { get; private set; }

        public object Value
        {
            get
            {
                return this.nameTextBox.Text;
            }
            set
            {
                var tuple = value as object[];
                if (tuple != null)
                {
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
