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

using SlipStream.Client.Agos.Models;

namespace SlipStream.Client.Agos.Windows.TreeView
{
    public interface IQueryField
    {
        QueryConstraint[] GetConstraints();
        void Empty();
        bool IsEmpty { get; }
        string FieldName { get; }
    }
}
