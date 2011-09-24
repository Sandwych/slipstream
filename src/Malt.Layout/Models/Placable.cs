using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Malt.Layout.Models
{
    [XmlType("placable")]
    public class Placable : IPlacable
    {
        public Placable()
        {
            this.ColumnSpan = 1;
            this.RowSpan = 1;
        }

        #region IPlacable 成员

        [XmlAttribute("rowspan")]
        public int RowSpan { get; set; }

        [XmlAttribute("colspan")]
        public int ColumnSpan { get; set; }

        #endregion
    }
}
