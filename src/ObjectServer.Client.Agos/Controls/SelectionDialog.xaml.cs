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

namespace ObjectServer.Client.Agos.Controls
{
    public partial class SelectionDialog : FloatableWindow
    {
        private readonly string modelName;

        public SelectionDialog(string modelName)
        {
            var app = (App)App.Current;
            this.ParentLayoutRoot = app.MainPage.LayoutRoot;
            InitializeComponent();

            this.modelName = modelName;
            this.treeView1.Init(this.modelName, null);
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

