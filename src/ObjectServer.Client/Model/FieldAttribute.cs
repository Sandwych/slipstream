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

namespace ObjectServer.Client.Model
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class FieldAttribute : Attribute
    {
        public FieldAttribute(string field)
        {
            this.Name = field;
        }

        public string Name { get; private set; }
    }
}
