namespace ObjectServer.Client.Agos
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Navigation;

    /// <summary>
    /// <see cref="UserControl"/> class providing the main UI for the application.
    /// </summary>
    public partial class MainPage : UserControl
    {
        /// <summary>
        /// Creates a new <see cref="MainPage"/> instance.
        /// </summary>
        public MainPage()
        {
            InitializeComponent();
            Loaded += new RoutedEventHandler(Page_Loaded);
        }

        /// <summary>
        /// After the Frame navigates, ensure the <see cref="HyperlinkButton"/> representing the current page is selected
        /// </summary>
        private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
        {

        }

        public void NavigateTo(Uri uri)
        {
            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }

            this.ContentFrame.Navigate(uri);
        }

        public void NavigateToByRelative(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException("path");
            }

            this.NavigateTo(new Uri(path, UriKind.Relative));
        }

        public void NavigateToContentPage()
        {
            var uri = new Uri("/Content", UriKind.Relative);
            this.ContentFrame.Navigate(uri);
        }

        public void NavigateToLoginPage()
        {
            var uri = new Uri("/Login", UriKind.Relative);
            this.ContentFrame.Navigate(uri);
        }

        public void NavigateToDatabasesPage()
        {
            var uri = new Uri("/Databases", UriKind.Relative);
            this.ContentFrame.Navigate(uri);
        }

        /// <summary>
        /// If an error occurs during navigation, show an error window
        /// </summary>
        private void ContentFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            e.Handled = true;
            ErrorWindow.CreateNew(e.Exception);
        }



        private void Page_Loaded(object sender, RoutedEventArgs e)
        {


        }
    }
}