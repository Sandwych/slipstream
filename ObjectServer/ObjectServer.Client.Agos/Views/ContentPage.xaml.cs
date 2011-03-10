namespace ObjectServer.Client.Agos
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Navigation;
    using System.Windows.Controls.Theming;

    using ObjectServer.Client;
    using ObjectServer.Client.Model;

    /// <summary>
    /// Home page for the application.
    /// </summary>
    public partial class ContentPage : Page
    {
        /// <summary>
        /// Creates a new <see cref="MainPage"/> instance.
        /// </summary>
        public ContentPage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Executes when the user navigates to this page.
        /// </summary>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var app = (App)Application.Current;
            this.Title = ApplicationStrings.HomePageTitle;
            this.TextUserName.Text = app.ClientService.LoggedUserName;
            this.TextServerUri.Text = app.ClientService.ServerAddress.ToString();
            app.IsBusy = true;

            app.ClientService.ReadAllMenus(menus =>
            {
                this.Menu.LoadMenus(menus);
                app.IsBusy = false;                    
            });
        }

        private void treeMenus_SelectedItemChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<object> e)
        {

        }




    }
}