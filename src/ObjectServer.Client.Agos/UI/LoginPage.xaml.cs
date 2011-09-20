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
using System.Collections;
using System.Threading;
using System.ComponentModel.DataAnnotations;

using ObjectServer.Client.Agos.Controls;
using ObjectServer.Client.Agos.Models;

namespace ObjectServer.Client.Agos.UI
{
    public partial class LoginPage : Page
    {
        public LoginPage()
        {
            InitializeComponent();

            this.textServer.Text = @"http://localhost:9287";
            this.textLogin.Text = "root";
            this.textPassword.Password = "root";
            this.LoadDatabaseList();

            this.Loaded += new RoutedEventHandler(LoginPage_Loaded);
        }


        void LoginPage_Loaded(object sender, RoutedEventArgs e)
        {
            this.DataContext = new LoginModel();
        }

        private void LoadDatabaseList()
        {
            var client = new ObjectServerClient(new System.Uri(this.textServer.Text));

            var app = (App)Application.Current;
            app.IsBusy = true;
            client.ListDatabases(dbs =>
            {
                this.listDatabases.ItemsSource = dbs;

                if (dbs.Length >= 1)
                {
                    this.listDatabases.SelectedIndex = 0;
                }

                app.IsBusy = false;
            });
        }

        // Executes when the user navigates to this page.
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {

        }

        private void buttonChangeServer_Click(object sender, RoutedEventArgs e)
        {

        }

        private void buttonSignIn_Click(object sender, RoutedEventArgs e)
        {
            Signin();
        }

        private void Signin()
        {

            var loginModel = (LoginModel)this.DataContext;

            var app = (App)Application.Current;
            app.IsBusy = true;

            var client = new ObjectServerClient(new Uri(this.textServer.Text));

            client.LogOn(loginModel.Database, loginModel.Login, loginModel.Password,
                sid =>
                {
                    if (string.IsNullOrEmpty(sid))
                    {
                        this.textMessage.Text = "登录失败，请检查用户名与密码是否正确";
                    }
                    else
                    {
                        app.ClientService = client;
                        app.MainPage.NavigateToContentPage();
                    }

                    app.IsBusy = false;
                });

        }

        private void Page_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && this.buttonSignIn.IsEnabled)
            {
                this.buttonSignIn_Click(sender, e);
            }
        }

        private void ButtonDatabaseManagement_Click(object sender, RoutedEventArgs e)
        {
            var app = (App)Application.Current;
            app.MainPage.NavigateToDatabasesPage();
        }

    }
}
