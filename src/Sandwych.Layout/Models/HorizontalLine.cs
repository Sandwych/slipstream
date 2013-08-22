using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Sandwych.Layout.Models
{
    [XmlType("hr")]
    public class HorizontalLine : Placable
    {
        public HorizontalLine()
        {
            this.ColumnSpan = 4;
            this.RowSpan = 1;
        }

        #region IPlacable 成员

        [XmlAttribute("text")]
        public string Text { get; set; }

        #endregion
    }
}
