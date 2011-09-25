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

using Malt.Layout.Widgets;

using ObjectServer.Client.Agos.Models;

namespace ObjectServer.Client.Agos.Windows.FormView
{
    public class HLine : Grid, IHorizontalLineWidget
    {
        Rectangle border;
        Label label;

        public HLine()
        {
            this.ColumnDefinitions.Add(
                new ColumnDefinition() { Width = GridLength.Auto });
            this.ColumnDefinitions.Add(
                new ColumnDefinition() { Width = new GridLength(100, GridUnitType.Star) });

            this.label = new Label();
            this.Children.Add(this.label);
            this.label.SetValue(Grid.ColumnProperty, 0);
            this.label.Content = this.Text ?? string.Empty;
            this.label.VerticalContentAlignment = System.Windows.VerticalAlignment.Center;
            this.label.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            this.label.Margin = new Thickness(0, 0, 5, 0);

            this.border = new Rectangle();
            this.Children.Add(this.border);
            this.border.SetValue(Grid.ColumnProperty, 1);
            this.border.Fill = new SolidColorBrush(Color.FromArgb(0xff, 0x99, 0x99, 0x99));
            this.border.Height = 1;
            this.border.VerticalAlignment = System.Windows.VerticalAlignment.Center;

            this.label.Content = string.Empty;
            this.Margin = new Thickness(5, 2, 5, 2);
        }

        public string Text
        {
            get
            {
                return (string)this.label.Content;
            }
            set
            {
                this.label.Content = value ?? string.Empty;
            }
        }
    }
}
