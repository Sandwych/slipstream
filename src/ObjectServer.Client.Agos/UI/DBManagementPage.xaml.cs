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
using System.Windows.Navigation;
using System.Threading;

namespace ObjectServer.Client.Agos.UI
{
    public partial class DBManagementPage : Page
    {
        public DBManagementPage()
        {
            InitializeComponent();
        }

        // Executes when the user navigates to this page.
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            var app = (App)Application.Current;
            app.MainPage.NavigateToLoginPage();
        }

        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {
            this.LoadDatabaseList();
        }

        private void LoadDatabaseList()
        {
            var app = (App)Application.Current;

            try
            {
                app.ClientService.BeginListDatabases((dbs) =>
                {
                    this.databases.ItemsSource = dbs;
                });
            }
            catch (System.Security.SecurityException)
            {
                ErrorWindow.CreateNew(
                    "安全错误：无法连接服务器，或服务器缺少 '/crossdomain.xml'文件。",
                    StackTracePolicy.OnlyWhenDebuggingOrRunningLocally);
            }
        }

        private void buttonNew_Click(object sender, RoutedEventArgs e)
        {
            var app = (App)Application.Current;
            app.MainPage.NavigateToByRelative("/DBCreation");
        }

        private void buttonDrop_Click(object sender, RoutedEventArgs e)
        {
            var dbName = this.databases.SelectedValue as string;
            var dlg = new DBDeletionDialog(dbName);
            dlg.Closed += this.passwordDlg_Closed;
            dlg.Show();
        }

        private void databases_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.buttonDrop.IsEnabled = this.databases.SelectedValue != null;
        }

        void creationDlg_Closed(object sender, EventArgs e)
        {

        }

        void passwordDlg_Closed(object sender, EventArgs e)
        {
            var dlg = (DBDeletionDialog)sender;

            if (dlg.DialogResult == true && !string.IsNullOrEmpty(dlg.rootPassword.Password))
            {
                var password = dlg.rootPassword.Password;
                var dbName = dlg.DBName;
                DeleteDatabase(password, dbName);
            }
        }

        private void DeleteDatabase(string password, string dbName)
        {
            var app = (App)Application.Current;
            app.IsBusy = true;
            try
            {
                app.ClientService.BeginDeleteDatabase(password, dbName, delegate
                {
                    app.IsBusy = false;
                    this.LoadDatabaseList();
                });
            }
            catch (JsonRpcException jre)
            {
                app.IsBusy = false;
                ErrorWindow.CreateNew(jre.Message);
            }
        }

    }
}
