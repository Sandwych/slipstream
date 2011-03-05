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

namespace ObjectServer.Client.Agos.Views.Login
{
    public partial class LoginPage : Page
    {

        public LoginPage()
        {
            InitializeComponent();

            var client = new ObjectServerClient(new System.Uri("http://localhost:9287/ObjectServer.ashx"));
            client.ListDatabases(dbs =>
            {
                this.listDatabases.ItemsSource = dbs;
                //this.ApplicationNameTextBlock.Text = version.ToString();
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
            DoSignin();
        }

        private void DoSignin()
        {
         
        }

    }
}
