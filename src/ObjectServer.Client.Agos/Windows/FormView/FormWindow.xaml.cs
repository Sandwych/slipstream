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
using System.Xml.Serialization;

using ObjectServer.Client.Agos.Models;
using ObjectServer.Client.Agos;

namespace ObjectServer.Client.Agos.Windows.FormView
{
    public partial class FormWindow : UserControl, IWindowAction
    {
        private IDictionary<string, object> viewRecord;
        private IDictionary<string, object> actionRecord;
        private readonly IList<string> fields = new List<string>();
        private string modelName;

        public FormWindow(long actionId)
        {
            InitializeComponent();

            this.Init();
        }

        public long ActionID
        {
            get;
            private set;
        }

        private void Init()
        {
            var app = (App)Application.Current;
            var actionIds = new long[] { this.ActionID };

            /*
            app.ClientService.ReadModel("core.action_window", actionIds, null, actionRecords =>

            {
                this.actionRecord = actionRecords[0];
                var view = (object[])actionRecords[0]["view"];
                var viewIds = new long[] { (long)view[0] };
                app.ClientService.ReadModel("core.view", viewIds, null, viewRecords =>
                {
                    this.viewRecord = viewRecords[0];
                });
            });
            */


            var args = new object[] { "core.user" };
            app.ClientService.Execute("core.model", "GetFields", args, result =>
            {
                var fields = ((object[])result).Select(r => (IDictionary<string, object>)r).ToArray();

                this.LoadForm(fields);
            });
        }

        private void LoadForm(IDictionary<string, object>[] fields)
        {
            //this.modelName = (string)this.actionRecord["model"];
            //var layout = (string)this.viewRecord["layout"];

            var app = (App)Application.Current;
            var layout =
@"<?xml version='1.0' encoding='utf-8' ?>
      <form label='Users' col='6'>
        <label field='name' />
        <field name='name' />
        <label field='login' />
        <field name='login' />
        <label field='admin' />
        <field name='admin' />
      </form>";

            var xs = new XmlSerializer(typeof(Malt.Layout.Models.Form));
            Malt.Layout.Models.Form form;
            using (var sr = new System.IO.StringReader(layout))
            {
                form = (Malt.Layout.Models.Form)xs.Deserialize(sr);
            }

            var factory = new FieldControlFactory(fields);
            var le = new Malt.Layout.LayoutEngine(factory);
            var layoutGrid = (UIElement)le.CreateLayoutTable(form);
            this.Content = layoutGrid;

        }
    }
}
