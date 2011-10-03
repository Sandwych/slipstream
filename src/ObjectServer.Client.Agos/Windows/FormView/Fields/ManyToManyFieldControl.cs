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
using System.Threading;
using System.Diagnostics;

using ObjectServer.Client.Agos.Models;

namespace ObjectServer.Client.Agos.Windows.FormView
{
    public class ManyToManyFieldControl : Grid, IFieldWidget
    {
        private readonly Grid rootLayout = new Grid();
        private readonly IDictionary<string, object> metaField;
        private readonly Border border;
        private readonly DataGrid treeView;
        private readonly Button addButton = new Button();
        private readonly Button removeButton = new Button();
        private string relatedModel;

        public ManyToManyFieldControl(object metaField)
        {
            var app = (App)Application.Current;

            this.metaField = (IDictionary<string, object>)metaField;
            this.FieldName = (string)this.metaField["name"];

            //载入数据

            this.RowDefinitions.Add(
                new RowDefinition()
                {
                    Height = new GridLength(100, GridUnitType.Star)
                });

            this.border = new Border();
            this.border.Child = this.rootLayout;
            this.Children.Add(this.border);
            this.border.SetValue(Grid.RowProperty, 0);
            this.border.BorderThickness = new Thickness(1);
            this.border.BorderBrush = new SolidColorBrush(Color.FromArgb(0xff, 0x99, 0x99, 0x99));

            this.rootLayout.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            this.rootLayout.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });

            this.addButton.Content = "添加...";
            this.addButton.Height = 23;
            this.addButton.Width = 75;
            this.addButton.Click += new RoutedEventHandler(AddButtonClicked);

            this.removeButton.Content = "移除";
            this.removeButton.Height = 23;
            this.removeButton.Width = 75;

            var toolbar = new StackPanel();
            toolbar.Margin = new Thickness(5, 5, 5, 5);
            toolbar.Orientation = Orientation.Horizontal;
            this.rootLayout.Children.Add(toolbar);
            toolbar.SetValue(Grid.RowProperty, 0);
            toolbar.Children.Add(this.addButton);
            toolbar.Children.Add(this.removeButton);

            this.treeView = new DataGrid();
            this.treeView.BorderThickness = new Thickness(0);
            this.rootLayout.Children.Add(this.treeView);
            this.treeView.SetValue(Grid.RowProperty, 1);


            var relatedFieldName = (string)this.metaField["related_field"];
            var getFieldsArgs = new object[] { (string)this.metaField["relation"] };
            app.ClientService.BeginExecute("core.model", "GetFields", getFieldsArgs, o =>
            {
                var fields = (object[])o;
                var relatedField =
                    (from i in fields
                     let f = (IDictionary<string, object>)i
                     where (string)f["name"] == relatedFieldName
                     select f).Single();
                this.relatedModel = (string)relatedField["relation"];
                Debug.Assert(!string.IsNullOrEmpty(this.relatedModel));
            });
        }

        public string FieldName { get; private set; }

        public object Value
        {
            get
            {
                return null;
            }
            set
            {
                Debug.Assert(!string.IsNullOrEmpty(this.relatedModel));
                var app = (App)Application.Current;
                var refIDs = (object[])value;


                var args = new object[] { refIDs, null };
                app.ClientService.BeginExecute(this.relatedModel, "Read", args, o2 =>
                {
                    var objs = (object[])o2;
                    var records = objs.Select(r => (Dictionary<string, object>)r).ToArray();

                    this.treeView.ItemsSource = DataSourceCreator.ToDataSource(records);
                });
            }
        }

        public void Empty()
        {
        }

        public void AddButtonClicked(object sender, RoutedEventArgs args)
        {
            Debug.Assert(!string.IsNullOrEmpty(this.relatedModel));

            var dlg = new Controls.SelectionDialog(this.relatedModel);
            dlg.ShowDialog();
        }
    }
}
