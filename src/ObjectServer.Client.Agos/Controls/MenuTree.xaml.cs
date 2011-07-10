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

using ObjectServer.Client;
using ObjectServer.Client.Model;

namespace ObjectServer.Client.Agos.Controls
{
    public partial class MenuTree : UserControl
    {
        public MenuTree()
        {
            InitializeComponent();

            this.Tree.SelectedValuePath = "Id";
        }

        public void LoadMenus(IEnumerable<MenuModel> menus)
        {
            this.Tree.Items.Clear();
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
                this.Tree.Items.Add(node);
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
    }
}
