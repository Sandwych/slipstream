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

using SlipStream.Client.Agos.Models;

namespace SlipStream.Client.Agos.Windows.FormView
{
    public class DecimalFieldControl : UpDownBase<decimal?>, IFieldWidget
    {
        private readonly IDictionary<string, object> metaField;

        public DecimalFieldControl(object metaField)
        {
            this.metaField = (IDictionary<string, object>)metaField;
            this.FieldName = (string)this.metaField["name"];

            this.VerticalContentAlignment = System.Windows.VerticalAlignment.Center;
        }

        public string FieldName { get; private set; }

        public new object Value
        {
            get
            {
                return base.Value;
            }
            set
            {
                if (value != null)
                {
                    base.Value = (decimal)value;
                }
                else
                {
                    base.Value = null;
                }
            }
        }

        public void Empty()
        {
            this.Value = 0;
        }

        protected override string FormatValue()
        {
            if (base.Value == null)
            {
                return string.Empty;
            }
            else
            {
                return this.Value.ToString();
            }
        }

        protected override void OnDecrement()
        {
            var value = base.Value;
            base.Value -= 0.01M;
        }

        protected override void OnIncrement()
        {
            var value = base.Value;
            base.Value += 0.01M;
        }

        protected override decimal? ParseValue(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return null;
            }
            else
            {
                return decimal.Parse(text);
            }
        }
    }
}
