using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Data;
using System.Linq;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Diagnostics;

namespace ObjectServer.Client.Agos.Controls
{
    public class TreeDataGrid : DataGrid
    {
        private static Dictionary<string, Tuple<Type, IValueConverter>> COLUMN_TYPE_MAPPING
            = new Dictionary<string, Tuple<Type, IValueConverter>>()
        {
            {"id", new Tuple<Type, IValueConverter>(typeof(DataGridTextColumn), null) },
            {"int32", new Tuple<Type, IValueConverter>(typeof(DataGridTextColumn), null) },
            {"int64", new Tuple<Type, IValueConverter>(typeof(DataGridTextColumn), null) },
            {"float8", new Tuple<Type, IValueConverter>(typeof(DataGridTextColumn), null) },
            {"decimal", new Tuple<Type, IValueConverter>(typeof(DataGridTextColumn), null) },
            {"chars", new Tuple<Type, IValueConverter>(typeof(DataGridTextColumn), null) },
            {"text", new Tuple<Type, IValueConverter>(typeof(DataGridTextColumn), null) },
            {"boolean", new Tuple<Type, IValueConverter>(typeof(DataGridCheckBoxColumn), null) },
            {"datetime", new Tuple<Type, IValueConverter>(typeof( DataGridTextColumn),  new DateTimeFieldConverter()) },
            {"date", new Tuple<Type, IValueConverter>(typeof(DataGridTextColumn), new DateFieldConverter()) },
            {"time", new Tuple<Type, IValueConverter>(typeof(DataGridTextColumn), new TimeFieldConverter()) },
            {"many-to-one", new Tuple<Type, IValueConverter>(typeof(DataGridTextColumn), new ManyToOneFieldConverter()) },
            {"reference", new Tuple<Type, IValueConverter>(typeof(DataGridTextColumn), new ReferenceFieldConverter()) },
            {"enum", new Tuple<Type, IValueConverter>(typeof(DataGridTextColumn), new EnumFieldConverter()) },
        };

        private IDictionary<string, object> viewRecord;
        private readonly IList<string> fields = new List<string>();
        private string modelName;
        private readonly List<long> recordIds = new List<long>();

        public TreeDataGrid()
        {
            this.AutoGenerateColumns = false;
            this.IsReadOnly = true;
        }

        public void Reload(IEnumerable<long> ids)
        {
            this.recordIds.Clear();
            this.ItemsSource = null;

            if (ids.Count() > 0)
            {
                this.recordIds.AddRange(ids);
                this.LoadData();
            }
        }

        public void Init(string model, long? viewID)
        {
            if (string.IsNullOrEmpty(model))
            {
                throw new ArgumentNullException("model");
            }

            this.modelName = model;

            this.Columns.Clear();

            var app = (App)Application.Current;

            var getViewArgs = new object[] { this.modelName, "tree", viewID };
            app.ClientService.BeginExecute("core.view", "GetView", getViewArgs, (result, error) =>
            {
                this.viewRecord = (IDictionary<string, object>)result;
                this.LoadInternal();
            });
        }

        private void LoadData()
        {
            var app = (App)Application.Current;
            //加载数据
            var offset = 0;// long.Parse(this.textOffset.Text);
            var limit = 2000;// long.Parse(this.textLimit.Text);
            var constraints = new object[][] 
            {
                new object[] { "_id", "in", this.recordIds }
            };

            app.ClientService.SearchModel(this.modelName, constraints, null, offset, limit, (ids, error) =>
            {
                app.ClientService.ReadModel(this.modelName, ids, this.fields, records =>
                {
                    //我们需要一个唯一的字符串型 ID
                    this.ItemsSource = DataSourceCreator.ToDataSource(records);
                });
            });
        }

        private void LoadInternal()
        {
            var layout = (string)this.viewRecord["layout"];
            var layoutDocument = XDocument.Parse(layout);

            this.InitializeColumns(layoutDocument);
        }

        private void InitializeColumns(XDocument layoutDocument)
        {
            Debug.Assert(!string.IsNullOrEmpty(this.modelName));

            var app = (App)Application.Current;
            var args = new object[] { this.modelName };
            app.ClientService.BeginExecute("core.model", "GetFields", args, (result, error) =>
            {
                var metaFields = ((object[])result).Select(r => (Dictionary<string, object>)r).ToArray();
                var viewFields = layoutDocument.Elements("tree").Elements();

                IList<DataGridBoundColumn> cols = new List<DataGridBoundColumn>();
                cols.Add(this.MakeColumn("_id", "id", "ID", System.Windows.Visibility.Collapsed));

                foreach (var f in viewFields)
                {
                    var fieldName = f.Attribute("name").Value;
                    var metaField = metaFields.Single(i => (string)i["name"] == fieldName);
                    cols.Add(this.MakeColumn(fieldName, (string)metaField["type"], (string)metaField["label"]));
                }

                this.Columns.Clear();
                foreach (var col in cols)
                {
                    this.Columns.Add(col);
                }

            });
        }

        private DataGridBoundColumn MakeColumn(
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
            return col;
        }

        public long[] GetSelectedIDs()
        {
            var ids = new List<long>();
            foreach (dynamic item in this.SelectedItems)
            {
                var id = (long)item._id;
                ids.Add(id);
            }
            return ids.ToArray();
        }
    }
}
