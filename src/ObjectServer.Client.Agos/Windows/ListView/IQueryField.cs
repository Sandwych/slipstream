﻿using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

using ObjectServer.Client.Agos.Models;

namespace ObjectServer.Client.Agos.Windows.ListView
{
    public interface IQueryField
    {
        QueryConstraint[] GetConstraints();
        void Empty();
        bool IsEmpty { get; }
        string FieldName { get; }
    }
}
