using System;
using System.Collections;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace ObjectServer.Client.Agos.Models
{
    public class TreeViewModel
    {
        public TreeViewModel(long actionID)
        {
            this.ActionID = actionID;
        }

        public long ActionID { get; private set; }

        public IEnumerable Date { get; private set; }
    }
}
