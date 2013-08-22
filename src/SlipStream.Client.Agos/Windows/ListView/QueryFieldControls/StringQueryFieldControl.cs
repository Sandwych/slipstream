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
using System.Collections.Generic;

using SlipStream.Client.Agos.Models;

namespace SlipStream.Client.Agos.Windows.TreeView.QueryFieldControls
{
    public class StringQueryFieldControl : TextBox, IQueryField
    {
        private readonly IDictionary<string, object> metaField;

        public StringQueryFieldControl(object metaField)
            : base()
        {
            this.metaField = (IDictionary<string, object>)metaField;
            this.FieldName = (string)this.metaField["name"];
        }

        public QueryConstraint[] GetConstraints()
        {
            System.Diagnostics.Debug.Assert(!this.IsEmpty);
            var constraints = new QueryConstraint[] 
            {
                new QueryConstraint(this.FieldName, "like", "%" + this.Text + "%")
            };
            return constraints;
        }

        public void Empty()
        {
            this.Text = String.Empty;
        }

        public bool IsEmpty
        {
            get
            {
                return String.IsNullOrEmpty(this.Text);
            }
        }

        public string FieldName { get; private set; }
    }
}
