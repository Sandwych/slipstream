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

using SlipStream.Client.Agos.Controls;
using SlipStream.Client.Agos.Models;
using Sandwych.Json;

namespace SlipStream.Client.Agos.UI
{
    public partial class LoginPage : Page
    {
        public LoginPage()
        {
            InitializeComponent();
            this.buttonSignIn.IsEnabled = false;

            this.Loaded += new RoutedEventHandler(LoginPage_Loaded);
        }


        void LoginPage_Loaded(object sender, RoutedEventArgs e)
        {
            this.DataContext = new LoginModel();

            this.LoadDatabaseList();
        }

        private void LoadDatabaseList()
        {
            var app = (App)Application.Current;
            var loginModel = (LoginModel)this.DataContext;

            if (app.ClientService == null)
            {
                app.ClientService = new SlipStreamClient(new Uri(loginModel.Address));
            }

            try
            {
                app.ClientService.ListDatabases((dbs, error) =>
                {
                    this.listDatabases.ItemsSource = dbs;

                    if (dbs.Length >= 1)
                    {
                        this.buttonSignIn.IsEnabled = true;
                        this.listDatabases.SelectedIndex = 0;
                    }
                });
            }
            catch (System.Security.SecurityException)
            {
                ErrorWindow.CreateNew(
                    "安全错误：无法连接服务器，或服务器缺少 '/crossdomain.xml'文件。",
                    StackTracePolicy.OnlyWhenDebuggingOrRunningLocally);
            }
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

            var client = new SlipStreamClient(new Uri(this.textServer.Text));

            try
            {
                client.LogOn(loginModel.Database, loginModel.Login, loginModel.Password,
                    (sid, error) =>
                    {
                        app.IsBusy = false;
                        app.ClientService = client;
                        app.MainPage.NavigateToContentPage();
                    });
            }
            catch (JsonRpcException)
            {
                this.textMessage.Text = "登录失败，请检查用户名与密码是否正确";
            }

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
            app.MainPage.NavigateToByRelative("/DBManagement");
        }

    }
}
