using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Sandwych.Layout.Models
{
    [XmlType("placable")]
    public class Placable : IPlacable
    {
        public Placable()
        {
            this.ColumnSpan = 1;
            this.RowSpan = 1;
            this.Fill = false;
        }

        #region IPlacable 成员

        [XmlAttribute("rowspan")]
        public int RowSpan { get; set; }

        [XmlAttribute("colspan")]
        public int ColumnSpan { get; set; }

        [XmlAttribute("fill")]
        public bool Fill { get; set; }


        #endregion
    }
}
