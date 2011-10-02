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

namespace ObjectServer.Client.Agos.Windows.TreeView
{
    public partial class TreeWindow : UserControl
    {
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
                var modelName = (string)actionRecord["model"];

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
            this.TreeView.DeleteSelectedItems();
        }

        private void NewButton_Click(object sender, RoutedEventArgs e)
        {
            this.TreeView.New();
        }
    }
}
