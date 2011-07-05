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
            app.IsBusy = true;

            app.ClientService.ReadAllMenus(menus =>
            {
                this.LoadMenus(menus);
                app.IsBusy = false;
            });
        }

        private void Menu_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var app = App.Current as App;

            var tvi = this.Menu.SelectedItem as TreeViewItem;
            var menu = tvi.DataContext as Menu;

            /* if (menu.Action != null)
             {
                 //OpenActionTab(menu);
             }
             */

        }

        /*
        private void OpenActionTab(Menu menu)
        {
            //先看看有没有已经打开同样的动作标签页了，如果有就跳转过去
            var existedActionTab =
                (from TabItem tab in this.tabWindows.Items
                 where tab.DataContext is ReferenceField &&
                 ((ReferenceField)tab.DataContext).Id == menu.Action.Id
                 select tab).FirstOrDefault();

            if (existedActionTab != null)
            {
                this.tabWindows.SelectedItem = existedActionTab;
                return;
            }


            var actWin = new Views.WindowAction(
                this.objectService, menu.Action.Model, menu.Action.Id);

            var tabPage = new TabItem();
            tabPage.DataContext = menu.Action;
            tabPage.Content = actWin;
            tabPage.Header = menu.Action.Model;
            this.tabWindows.Items.Add(tabPage);
            this.tabWindows.SelectedItem = tabPage;
        }*/



        public void LoadMenus(IEnumerable<Menu> menus)
        {
            this.Menu.Items.Clear();
            this.InsertMenus(menus);
        }

        private void InsertMenus(IEnumerable<Menu> menus)
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
                this.Menu.Items.Add(node);
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