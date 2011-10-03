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

namespace ObjectServer.Client.Agos.UI
{
    public partial class DBDeletionDialog : ChildWindow
    {
        public DBDeletionDialog()
        {
            InitializeComponent();
        }

        public DBDeletionDialog(string dbName)
            : this()
        {
            this.DBName = dbName;
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        public string DBName { get; private set; }
    }
}

