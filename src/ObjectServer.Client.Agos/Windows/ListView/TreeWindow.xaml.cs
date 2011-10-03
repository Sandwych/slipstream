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
    public partial class TreeWindow : UserControl
    {
        private string modelName;

        public TreeWindow(long actionID)
        {
            InitializeComponent();

            this.Init(actionID);
        }


        private void Init(long actionID)
        {
            var app = (App)Application.Current;
            var actionIds = new long[] { actionID };
            var fields = new string[] { "_id", "name", "view", "model", "views" };
            app.ClientService.ReadModel("core.action_window", actionIds, fields, actionRecords =>
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
            this.TreeView.Query();
        }

        private void QueryButton_Click(object sender, RoutedEventArgs e)
        {
            this.TreeView.Query();
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            this.TreeView.EditSelectedItem();
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
                app.IsBusy = true;

                var args = new object[] { ids };
                app.ClientService.BeginExecute(this.modelName, "Delete", args, result =>
                {
                    this.TreeView.Query();
                    app.IsBusy = false;
                });
            }
        }

        private void NewButton_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new FormView.FormDialog(this.modelName, -1);
            dlg.ShowDialog();
        }

    }
}
