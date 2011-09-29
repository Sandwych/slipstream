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
using System.Collections;
using System.Linq;

using ObjectServer.Client.Agos.Models;

namespace ObjectServer.Client.Agos.Windows.ListView.QueryFieldControls
{
    public class EnumerationQueryFieldControl : ComboBox, IQueryField
    {
        private readonly IDictionary<string, object> metaField;
        private static string NullKey = string.Empty;

        public EnumerationQueryFieldControl(object metaField)
            : base()
        {
            this.metaField = (IDictionary<string, object>)metaField;
            this.FieldName = (string)this.metaField["name"];

            this.VerticalAlignment = System.Windows.VerticalAlignment.Center;

            this.BindOptions();
        }

        private void BindOptions()
        {
            var options = (IEnumerable)this.metaField["options"];
            this.SelectedValuePath = "Key";
            this.DisplayMemberPath = "Value";
            this.ItemsSource = options;
        }

        public QueryConstraint[] GetConstraints()
        {
            System.Diagnostics.Debug.Assert(!this.IsEmpty);

            var constraints = new QueryConstraint[] 
            {
                new QueryConstraint(this.FieldName, "=", this.SelectedValue)
            };
            return constraints;
        }

        public void Empty()
        {
            this.SelectedIndex = -1;
        }

        public bool IsEmpty
        {
            get { return this.SelectedIndex < 0; }
        }

        public string FieldName { get; private set; }
    }
}
