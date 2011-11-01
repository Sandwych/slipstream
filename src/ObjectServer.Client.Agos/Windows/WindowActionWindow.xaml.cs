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

namespace ObjectServer.Client.Agos.Windows.TreeView
{
    public partial class WindowActionWindow : UserControl
    {
        private string modelName;

        public WindowActionWindow(long actionID)
        {
            InitializeComponent();

            this.Init(actionID);
        }


        private void Init(long actionID)
        {
            var app = (App)Application.Current;
            var actionIds = new long[] { actionID };
            var fields = new string[] { "_id", "name", "view", "model", "views" };
            app.ClientService.ReadModel("core.action_window", actionIds, fields, (actionRecords, error) =>
            {
                var actionRecord = actionRecords[0];
                this.modelName = (string)actionRecord["model"];

                var viewField = actionRecords[0]["view"] as object[];
                long? viewID;
                if (viewField == null)
                {
                    viewID = null;
                }
                else
                {
                    viewID = (long)viewField[0];
                }

                this.TreeView.Init(modelName, viewID);
            });
        }

        private void ClearConstraintsButton_Click(object sender, RoutedEventArgs e)
        {
            this.TreeView.ClearAllConstraints();
            this.TreeView.Clear();
        }

        private void QueryButton_Click(object sender, RoutedEventArgs e)
        {
            this.TreeView.Query();
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var ids = this.TreeView.GetSelectedIDs();
            if (ids.Length == 0)
            {
                return;
            }
            var dlg = new FormView.FormDialog(this.modelName, ids.First());
            dlg.ParentLayoutRoot = this.LayoutRoot;
            dlg.Saved += new EventHandler(this.OnSaved);
            dlg.ShowDialog();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var ids = this.TreeView.GetSelectedIDs();
            if (ids.Length == 0)
            {
                return;
            }

            var msg = String.Format("您确定要永久删除 {0} 条记录吗？", ids.Length);
            var dlgResult = MessageBox.Show(msg, "删除确认", MessageBoxButton.OKCancel);

            if (dlgResult == MessageBoxResult.OK)
            {
                //执行删除
                var app = (App)Application.Current;

                var args = new object[] { ids };
                app.ClientService.Execute(this.modelName, "Delete", args, (result, error) =>
                {
                    this.TreeView.Query();
                });
            }
        }

        private void NewButton_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new FormView.FormDialog(this.modelName, -1);
            dlg.ParentLayoutRoot = this.LayoutRoot;
            dlg.Saved += new EventHandler(this.OnSaved);
            dlg.ShowDialog();
        }

        private void OnSaved(object sender, EventArgs args)
        {
            this.TreeView.Query();
        }

    }
}
