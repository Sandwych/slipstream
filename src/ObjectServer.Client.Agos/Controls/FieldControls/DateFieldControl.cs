using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace ObjectServer.Client.Agos.Controls.FieldControls
{
    public class DateFieldControl : DatePicker, IFieldControl
    {
        public DateFieldControl()
        {
            this.DefaultStyleKey = typeof(DateFieldControl);
        }

        public DateFieldControl(string field)
            : this()
        {
            if (string.IsNullOrEmpty(field))
            {
                throw new ArgumentNullException("field");
            }

            this.BindedField = field;
        }

        #region IFieldControl Members

        public string BindedField { get; private set; }

        public object Value
        {
            get
            {
                return this.SelectedDate;
            }
            set
            {
                this.SelectedDate = (DateTime)value;
            }
        }

        public void Clear()
        {
            this.SelectedDate = null;
        }

        #endregion
    }
}
