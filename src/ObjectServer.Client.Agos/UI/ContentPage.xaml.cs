namespace ObjectServer.Client.Agos.UI
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Navigation;
    using System.Threading;

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
            this.Menu.SelectedValuePath = "Id";
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
        }

        private void Menu_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var app = App.Current as App;

            var tvi = this.Menu.SelectedItem as TreeViewItem;
            var menu = tvi.DataContext as MenuEntity;

            if (menu.Action != null)
            {
                this.OpenActionTab(menu);
            }
        }


        private void OpenActionTab(MenuEntity menu)
        {
            var tabPage = new TabItem();
            tabPage.DataContext = menu.Action;
            tabPage.Header = "ListWindow";
            this.TabContainer.Items.Add(tabPage);
            this.TabContainer.SelectedItem = tabPage;

            //先看看有没有已经打开同样的动作标签页了，如果有就跳转过去
            var actWin = new Windows.TreeView.WindowActionWindow(menu.Action.Item2);

            var actionName = menu.Action.Item1;

            tabPage.Content = actWin;
        }



        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            var app = (App)Application.Current;

            app.ClientService.LogOff(delegate
            {
                this.TabContainer.Items.Clear();
                app.PrepareToLogin();
                GC.Collect();
            });
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {


        }

    }
}
