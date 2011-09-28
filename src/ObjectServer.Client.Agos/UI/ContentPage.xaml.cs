namespace ObjectServer.Client.Agos.UI
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Navigation;

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

            app.ClientService.ReadAllMenus(menus =>
            {
                this.LoadMenus(menus);
            });
        }

        private void Menu_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var app = App.Current as App;

            var tvi = this.Menu.SelectedItem as TreeViewItem;
            var menu = tvi.DataContext as MenuModel;

            if (menu.Action != null)
            {
                this.OpenActionTab(menu);
            }
        }


        private void OpenActionTab(MenuModel menu)
        {
            var tabPage = new TabItem();
            tabPage.DataContext = menu.Action;
            tabPage.Header = "ListWindow";
            this.TabContainer.Items.Add(tabPage);
            this.TabContainer.SelectedItem = tabPage;

            //先看看有没有已经打开同样的动作标签页了，如果有就跳转过去
            var actWin = new Windows.ListView.ListView(menu.Action.Item2);

            var actionName = menu.Action.Item1;

            tabPage.Content = actWin;
        }


        public void LoadMenus(IEnumerable<MenuModel> menus)
        {
            this.Menu.Items.Clear();
            this.InsertMenus(menus);
        }

        private void InsertMenus(IEnumerable<MenuModel> menus)
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


        private TreeViewItem InsertMenu(TreeViewItem parent, MenuModel menu)
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
                this.Menu.Items.Add(node);
            }
            return node;
        }

        private void InsertSubmenus(
            IEnumerable<MenuModel> menus, MenuModel parentMenu, TreeViewItem parentNode)
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

    }
}