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

namespace ObjectServer.Client.Agos.UI
{
    public partial class DatabasesPage : Page
    {
        public DatabasesPage()
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
            var client = new ObjectServerClient(new System.Uri(@"http://localhost:9287"));

            var app = (App)Application.Current;
            app.IsBusy = true;
            client.ListDatabases(dbs =>
            {
                this.databases.ItemsSource = dbs;
                app.IsBusy = false;
            });
        }

        private void buttonNew_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new DatabaseCreationDialog();
            dlg.Closed += this.creationDlg_Closed;
            dlg.Show();
        }

        private void buttonDrop_Click(object sender, RoutedEventArgs e)
        {
            var dbName = this.databases.SelectedValue as string;
            var dlg = new DatabaseDeletionDialog(dbName);
            dlg.Closed += this.passwordDlg_Closed;
            dlg.Show();
        }

        private void databases_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.buttonDrop.IsEnabled = this.databases.SelectedValue != null;
        }

        void creationDlg_Closed(object sender, EventArgs e)
        {
            var dlg = (DatabaseCreationDialog)sender;
            var model = (Models.DBCreationModel)dlg.DataContext;

            if (dlg.DialogResult == true)
            {
                var app = (App)Application.Current;
                app.IsBusy = true;
                var client = new ObjectServerClient(new System.Uri(@"http://localhost:9287"));
                client.CreateDatabase(model.ServerPassword, model.DBName, model.AdminPassword, () =>
                {
                    this.LoadDatabaseList();
                    app.IsBusy = false;
                });
            }
        }

        void passwordDlg_Closed(object sender, EventArgs e)
        {
            var dlg = (DatabaseDeletionDialog)sender;

            if (dlg.DialogResult == true && !string.IsNullOrEmpty(dlg.rootPassword.Password))
            {
                var password = dlg.rootPassword.Password;
                var dbName = dlg.DatabaseName;
                DeleteDatabase(password, dbName);
            }
        }

        private void DeleteDatabase(string password, string dbName)
        {
            var app = (App)Application.Current;
            app.IsBusy = true;
            var client = new ObjectServerClient(new System.Uri(@"http://localhost:9287"));
            client.DeleteDatabase(password, dbName, () =>
            {
                this.LoadDatabaseList();
                app.IsBusy = false;
            });
        }


    }
}
