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
using System.Xml;
using System.Xml.Linq;

using ObjectServer.Client.Agos.Models;
using ObjectServer.Client.Agos;

namespace ObjectServer.Client.Agos.Windows
{
    public partial class ListWindow : UserControl, IWindowAction
    {
        public ListWindow()
        {
            InitializeComponent();
        }

        #region IWindowAction Members

        public void Load(string model, long actionId)
        {
            var app = (App)Application.Current;
            var ids = new long[] { actionId };
            app.ClientService.ReadModel("core.action_window", ids, null, records =>
            {
                this.LoadInternal(model, records[0]);
            });
        }

        private void LoadInternal(string model, IDictionary<string, object> record)
        {
            var app = (App)Application.Current;

            var layout = (string)record["layout"];
            var layoutDoc = new XDocument(layout);

            var domain = new object[][] { new object[] { "name", "=", model } };

            app.ClientService.SearchModel("core.field", domain, null, 0, 0, ids =>
            {
                var viewFields = layoutDoc.Elements("form").Elements();
                foreach (var f in viewFields)
                {
                    var col = new DataGridTextColumn();
                    col.Header = f.Attribute("name").Value;
                    this.gridList.Columns.Add(col);
                }
            });

        }

        #endregion
    }
}
