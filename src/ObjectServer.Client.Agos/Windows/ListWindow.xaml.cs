using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Data;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Xml;
using System.Xml.Linq;
using System.Diagnostics;

using ObjectServer.Client.Agos.Models;
using ObjectServer.Client.Agos;

namespace ObjectServer.Client.Agos.Windows
{
    public partial class ListWindow : UserControl, IWindowAction
    {
        sealed class ManyToOneFieldConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                if (value != null)
                {
                    var objs = (object[])value;
                    return objs[1];
                }
                else
                {
                    return value;
                }
            }

            public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                throw new NotSupportedException();
            }
        }

        sealed class EnumFieldConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                if (value != null)
                {
                    var objs = (object[])value;
                    return objs[1];
                }
                else
                {
                    return value;
                }
            }

            public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                throw new NotSupportedException();
            }
        }

        private static Dictionary<string, Tuple<Type, IValueConverter>> COLUMN_TYPE_MAPPING
            = new Dictionary<string, Tuple<Type, IValueConverter>>()
        {
            {"ID", new Tuple<Type, IValueConverter>(typeof(DataGridTextColumn), null) },
            {"Integer", new Tuple<Type, IValueConverter>(typeof(DataGridTextColumn), null) },
            {"Float", new Tuple<Type, IValueConverter>(typeof(DataGridTextColumn), null) },
            {"Chars", new Tuple<Type, IValueConverter>(typeof(DataGridTextColumn), null) },
            {"Boolean", new Tuple<Type, IValueConverter>(typeof(DataGridCheckBoxColumn), null) },
            {"DateTime", new Tuple<Type, IValueConverter>(typeof(DataGridTextColumn), null) },
            {"ManyToOne", new Tuple<Type, IValueConverter>(typeof(DataGridTextColumn), new ManyToOneFieldConverter()) },
            {"Enumeration", new Tuple<Type, IValueConverter>(typeof(DataGridTextColumn), new EnumFieldConverter()) },
        };

        private IDictionary<string, object> viewRecord;
        private IDictionary<string, object> actionRecord;
        private readonly IList<string> fields = new List<string>();
        private string modelName;

        public ListWindow()
        {
            this.ActionID = -1;

            InitializeComponent();
        }

        public ListWindow(long actionID)
        {
            this.ActionID = actionID;

            this.Init();

            this.InitializeComponent();
        }

        private void Init()
        {
            var app = (App)Application.Current;
            var actionIds = new long[] { this.ActionID };
            app.ClientService.ReadModel("core.action_window", actionIds, null, actionRecords =>
            {
                this.actionRecord = actionRecords[0];
                var view = (object[])actionRecords[0]["view"];
                var viewIds = new long[] { (long)view[0] };
                app.ClientService.ReadModel("core.view", viewIds, null, viewRecords =>
                {
                    this.viewRecord = viewRecords[0];
                    this.LoadInternal();
                });
            });
        }

        #region IWindowAction Members

        public long ActionID { get; private set; }

        #endregion

        private void LoadData()
        {
            var app = (App)Application.Current;
            //加载数据
            var offset = 0;// long.Parse(this.textOffset.Text);
            var limit = 500;// long.Parse(this.textLimit.Text);

            app.ClientService.SearchModel(this.modelName, null, null, offset, limit, ids =>
            {
                app.ClientService.ReadModel(this.modelName, ids, this.fields, records =>
                {
                    //我们需要一个唯一的字符串型 ID
                    var typeid = Guid.NewGuid().ToString();
                    this.gridList.ItemsSource = DataSourceCreator.ToDataSource(records, typeid, fields.ToArray());
                });
            });
        }

        private void LoadInternal()
        {
            var app = (App)Application.Current;

            var layout = (string)this.viewRecord["layout"];
            var layoutDoc = XDocument.Parse(layout);
            this.modelName = (string)this.actionRecord["model"];

            this.InitializeColumns(layoutDoc);

            this.LoadData();
        }

        private void InitializeColumns(XDocument layoutDoc)
        {
            var app = (App)Application.Current;
            var args = new object[] { this.modelName };
            app.ClientService.Execute("core.model", "GetFields", args, result =>
            {
                var fields = ((object[])result).Select(r => (Dictionary<string, object>)r);
                var viewFields = layoutDoc.Elements("tree").Elements();

                this.AddColumn("_id", "ID", "ID", System.Windows.Visibility.Collapsed);

                foreach (var f in viewFields)
                {
                    var fieldName = f.Attribute("name").Value;
                    var metaField = fields.Single(i => (string)i["name"] == fieldName);
                    this.AddColumn(fieldName, (string)metaField["type"], (string)metaField["label"]);
                }
            });
        }


        private void AddColumn(
            string fieldName, string fieldType, string fieldLabel, Visibility visibility = Visibility.Visible)
        {
            this.fields.Add(fieldName);
            var tuple = COLUMN_TYPE_MAPPING[fieldType];
            var col = Activator.CreateInstance(tuple.Item1) as DataGridBoundColumn;
            col.Visibility = visibility;
            col.Header = fieldLabel;
            col.Binding = new System.Windows.Data.Binding(fieldName);
            if (tuple.Item2 != null)
            {
                col.Binding.Converter = tuple.Item2;
            }
            this.gridList.Columns.Add(col);

        }

        private void buttonQuery_Click(object sender, RoutedEventArgs e)
        {
            Debug.Assert(this.ActionID > 0);
            this.LoadData();
        }

        private void buttonDelete_Click(object sender, RoutedEventArgs e)
        {
            var sb = new System.Text.StringBuilder();
            var ids = new List<long>();
            foreach (dynamic item in this.gridList.SelectedItems)
            {
                var id = (long)item._id;
                ids.Add(id);
            }

            var msg = String.Format("您确定要永久删除 {0} 条记录吗？", ids.Count);
            var dlgResult = MessageBox.Show(msg, "删除确认", MessageBoxButton.OKCancel);

            if (dlgResult == MessageBoxResult.OK)
            {
                //执行删除
                var app = (App)Application.Current;
                app.IsBusy = true;

                var args = new object[] { ids };
                app.ClientService.Execute(this.modelName, "Delete", args, result =>
                {
                    this.LoadData();
                    app.IsBusy = false;
                });
            }
        }

    }
}
