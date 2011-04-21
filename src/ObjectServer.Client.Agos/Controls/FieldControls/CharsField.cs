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
    public class CharsField : TextBox, IFieldControl
    {
        public CharsField()
        {
            this.DefaultStyleKey = typeof(CharsField);
        }

        public CharsField(string field)
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
                return this.Text;
            }
            set
            {
                var strValue = value as string;
                if (strValue == null)
                {
                    throw new ArgumentException("Value must be a string");
                }
                this.Text = strValue;
            }
        }

        public void Clear()
        {
            this.Value = string.Empty;
        }

        #endregion
    }
}
