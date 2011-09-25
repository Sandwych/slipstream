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
    public class ManyToManyFieldControl : Grid, IFieldWidget
    {
        private readonly IDictionary<string, object> metaField;
        private readonly Border border;
        private readonly DataGrid grid;

        public ManyToManyFieldControl(object metaField)
        {
            var app = (App)Application.Current;

            this.metaField = (IDictionary<string, object>)metaField;
            this.FieldName = (string)this.metaField["name"];

            //载入数据

            this.MinHeight = 120;

            this.RowDefinitions.Add(
                new RowDefinition()
                {
                    Height = new GridLength(100, GridUnitType.Star)
                });

            this.border = new Border();
            this.Children.Add(this.border);
            this.border.SetValue(Grid.RowProperty, 0);
            this.border.BorderThickness = new Thickness(1);
            this.border.BorderBrush = new SolidColorBrush(Color.FromArgb(0xff, 0x99, 0x99, 0x99));

            this.grid = new DataGrid();
            this.grid.BorderThickness = new Thickness(0);
            this.border.Child = this.grid;


            this.Margin = new Thickness(5, 2, 5, 2);
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
                var app = (App)Application.Current;
                var refIDs = (object[])value;

                var relatedFieldName = (string)this.metaField["related_field"];
                var getFieldsArgs = new object[] { (string)this.metaField["relation"] };
                app.ClientService.Execute("core.model", "GetFields", getFieldsArgs, o =>
                    {
                        var fields = (object[])o;
                        var relatedField =
                            (from i in fields
                             let f = (IDictionary<string, object>)i
                             where (string)f["name"] == relatedFieldName
                             select f).Single();
                        var relatedModel = (string)relatedField["relation"];

                        var args = new object[] { refIDs, null };
                        app.ClientService.Execute(relatedModel, "Read", args, o2 =>
                            {
                                var objs = (object[])o2;
                                var records = objs.Select(r => (Dictionary<string, object>)r).ToArray();
                                this.grid.ItemsSource = DataSourceCreator.ToDataSource(
                                    records, relatedModel, new string[] { "name" });
                            });
                    });

            }
        }

        public void Empty()
        {
        }
    }
}
