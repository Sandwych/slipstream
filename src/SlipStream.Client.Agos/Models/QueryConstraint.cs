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

namespace SlipStream.Client.Agos.Models
{
    public class QueryConstraint
    {
        public QueryConstraint(string field, string opr, object value)
        {
            this.Field = field;
            this.Operator = opr;
            this.Value = value;
        }

        public string Field { get; private set; }

        public string Operator { get; private set; }

        public object Value { get; private set; }

        public object[] ToConstraint()
        {
            return new object[] { this.Field, this.Operator, this.Value };
        }
    }
}
