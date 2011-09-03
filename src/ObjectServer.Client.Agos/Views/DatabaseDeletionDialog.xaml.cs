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

namespace ObjectServer.Client.Agos.Views
{
    public partial class DatabaseDeletionDialog : ChildWindow
    {
        public DatabaseDeletionDialog()
        {
            InitializeComponent();
        }

        public DatabaseDeletionDialog(string dbName)
            : this()
        {
            this.DatabaseName = dbName;
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        public string DatabaseName { get; private set; }
    }
}

