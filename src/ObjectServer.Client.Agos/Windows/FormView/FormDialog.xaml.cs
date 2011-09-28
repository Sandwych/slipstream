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

namespace ObjectServer.Client.Agos.Windows.FormView
{
    public partial class FormDialog : FloatableWindow
    {
        public FormDialog(string model, long recordID, IDictionary<string, object> action)
        {
            var app = (App)App.Current;
            this.ParentLayoutRoot = app.MainPage.LayoutRoot;

            InitializeComponent();

            var formWindow = new FormView(model, recordID, action);
            this.ScrollContent.Content = formWindow;
            /*
            this.LayoutRoot.Children.Add(formWindow);
            formWindow.SetValue(Grid.ColumnProperty, 0);
            formWindow.SetValue(Grid.RowProperty, 0);
            */
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}

