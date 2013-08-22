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

namespace SlipStream.Client.Agos.Controls
{
    public class RecordsSelectedEventArgs : EventArgs
    {
        public RecordsSelectedEventArgs(long[] ids)
        {
            this.SelectedIDs = ids;
        }

        public long[] SelectedIDs { get; private set; }
    }
}
