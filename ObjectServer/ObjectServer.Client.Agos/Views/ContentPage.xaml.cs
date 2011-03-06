namespace ObjectServer.Client.Agos
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Navigation;
    using System.Windows.Controls.Theming;

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
        }

        private void treeMenus_SelectedItemChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<object> e)
        {

        }
    }
}