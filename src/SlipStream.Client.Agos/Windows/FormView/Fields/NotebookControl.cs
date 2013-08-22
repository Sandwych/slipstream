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

namespace SlipStream.Client.Agos.Windows.FormView
{
    public sealed class NotebookControl : TabControl
    {
        public NotebookControl()
            : base()
        {
            this.Margin = new Thickness(5, 2, 5, 2);
        }
    }
}
