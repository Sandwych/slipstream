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
using System.Diagnostics;

using ObjectServer.Client.Agos.Models;

namespace ObjectServer.Client.Agos.Windows.FormView
{
    public class ManyToOneFieldControl : UserControl, IFieldWidget
    {
        private readonly IDictionary<string, object> metaField;

        private readonly Button selectionButton;
        private readonly TextBox nameTextBox;

        public ManyToOneFieldControl(object metaField)
        {
            var layoutRoot = new Grid();
            this.Content = layoutRoot;
            var col1 = new ColumnDefinition() { Width = new GridLength(100, GridUnitType.Star) };
            var col2 = new ColumnDefinition() { Width = GridLength.Auto, };
            layoutRoot.ColumnDefinitions.Add(col1);
            layoutRoot.ColumnDefinitions.Add(col2);

            this.nameTextBox = new TextBox();
            this.nameTextBox.SetValue(Grid.ColumnProperty, 0);
            this.nameTextBox.VerticalContentAlignment = System.Windows.VerticalAlignment.Center;
            this.nameTextBox.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            layoutRoot.Children.Add(nameTextBox);

            this.selectionButton = new Button();
            this.selectionButton.SetValue(Grid.ColumnProperty, 1);
            this.selectionButton.Content = "...";
            this.selectionButton.Click += new RoutedEventHandler(this.SelectionButtonClicked);
            layoutRoot.Children.Add(selectionButton);

            this.metaField = (IDictionary<string, object>)metaField;
            this.FieldName = (string)this.metaField["name"];

            this.VerticalContentAlignment = System.Windows.VerticalAlignment.Center;
            this.VerticalAlignment = System.Windows.VerticalAlignment.Center;
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
                    this.nameTextBox.Text = (string)tuple[1];
                }
            }
        }

        public void Empty()
        {
        }

        public void SelectionButtonClicked(object sender, RoutedEventArgs args)
        {
            Debug.Assert(this.metaField != null);
            var relatedModel = this.metaField["relation"] as string;
            Debug.Assert(!string.IsNullOrEmpty(relatedModel));

            var dlg = new Controls.SelectionDialog(relatedModel);
            dlg.ShowDialog();
        }
    }
}
