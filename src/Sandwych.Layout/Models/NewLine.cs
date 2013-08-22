using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Sandwych.Layout.Models
{
    [XmlType("br")]
    public sealed class NewLine : Placable
    {
        public NewLine()
        {
            this.ColumnSpan = 0;
            this.RowSpan = 1;
        }

        #region IPlacable 成员

        [XmlAttribute("text")]
        public string Text { get; set; }

        #endregion
    }
}
