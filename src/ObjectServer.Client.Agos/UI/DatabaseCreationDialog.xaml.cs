﻿using System;
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

using ObjectServer.Client.Agos.Models;

namespace ObjectServer.Client.Agos.UI
{
    public partial class DatabaseCreationDialog : ChildWindow
    {
        public DatabaseCreationDialog()
        {
            InitializeComponent();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {
            this.DataContext = new DBCreationModel();
        }
    }
}

