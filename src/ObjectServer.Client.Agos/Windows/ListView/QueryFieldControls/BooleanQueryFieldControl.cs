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

using ObjectServer.Client.Agos.Models;

namespace ObjectServer.Client.Agos.Windows.TreeView.QueryFieldControls
{
    public class BooleanQueryFieldControl : CheckBox, IQueryField
    {
        private readonly IDictionary<string, object> metaField;
          
        public BooleanQueryFieldControl(object metaField)
            : base()
        {
            this.metaField = (IDictionary<string, object>)metaField;
            this.FieldName = (string)this.metaField["name"];

            this.IsThreeState = true;
            this.VerticalContentAlignment = System.Windows.VerticalAlignment.Center;
            this.VerticalAlignment= System.Windows.VerticalAlignment.Center;
        }

        public QueryConstraint[] GetConstraints()
        {
            System.Diagnostics.Debug.Assert(!this.IsEmpty);
            var contraints = new QueryConstraint[] 
            { 
                new QueryConstraint(this.FieldName, "=", this.IsChecked.Value) 
            };

            return contraints;
        }

        public void Empty()
        {
            this.IsChecked = null;
        }

        public bool IsEmpty
        {
            get { return this.IsChecked == null; }
        }

        public string FieldName { get; private set; }
    }
}
