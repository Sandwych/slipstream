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

            this.treeMenus.SelectedValuePath = "Id";
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

            this.treeMenus.Items.Clear();

            app.ClientService.ReadAllMenus(menus =>
            {
                this.InsertRootMenus(menus);
            });
        }

        private void treeMenus_SelectedItemChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<object> e)
        {

        }

        private void InsertRootMenus(IEnumerable<Menu> menus)
        {
            var rootMenus =
                from m in menus
                where m.ParentId == null
                orderby m.Ordinal
                select m;

            foreach (var menu in rootMenus)
            {
                var node = InsertMenu(null, menu);

                InsertSubmenus(menus, menu, node);
            }
        }


        private TreeViewItem InsertMenu(TreeViewItem parent, Menu menu)
        {
            var node = new TreeViewItem();
            node.Header = menu.Name;
            node.DataContext = menu;
            node.DisplayMemberPath = "Name";

            if (parent != null)
            {
                parent.Items.Add(node);
            }
            else
            {
                this.treeMenus.Items.Add(node);
            }
            return node;
        }

        private void InsertSubmenus(
            IEnumerable<Menu> menus, Menu parentMenu, TreeViewItem parentNode)
        {
            //子菜单们
            var submenus =
                from m in menus
                where m.ParentId != null && m.ParentId == parentMenu.Id
                orderby m.Ordinal
                select m;

            foreach (var menu in submenus)
            {
                var node = InsertMenu(parentNode, menu);
                //再把子子菜单们找出来
                InsertSubmenus(menus, menu, node);
            }
        }


    }
}