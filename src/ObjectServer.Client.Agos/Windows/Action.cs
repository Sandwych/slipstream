using System;
using System.Net;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace ObjectServer.Client.Agos
{
    public class Action
    {
        public Action(long id)
        {
            this.ID = id;    
        }

        public long ID { get; private set; }

        public void Execute()
        {
        }

    }
}
