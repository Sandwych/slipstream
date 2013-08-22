using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

using Sandwych.Layout.Models;

namespace SlipStream.Client.Agos.Windows.FormView
{
    public sealed class PageControl : TabItem
    {
        public PageControl(string label)
            : base()
        {
            if (string.IsNullOrEmpty(label))
            {
                throw new ArgumentNullException("label");
            }

            this.Header = label;
        }
    }
}
