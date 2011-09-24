using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Malt.Layout.Models
{
    [XmlType("form")]
    public class Form : IContainer
    {
        public Form()
        {
            this.ColumnCount = 6;
            this.ColumnSpan = 1;
            this.RowSpan = 1;
        }

        #region IContainer 成员

        [XmlElement(ElementName = "label", IsNullable = true, Type = typeof(Label))]
        [XmlElement(ElementName = "field", IsNullable = true, Type = typeof(Field))]
        [XmlElement(ElementName = "br", IsNullable = true, Type = typeof(NewLine))]
        [XmlElement(ElementName = "placeholder", IsNullable = true, Type = typeof(PlaceHolder))]
        [XmlElement(ElementName = "hr", IsNullable = true, Type = typeof(HorizontalLine))]
        public Placable[] ChildElements
        {
            get;
            set;
        }

        [XmlAttribute("col")]
        public int ColumnCount { get; set; }

        #endregion

        #region IPlacable 成员

        [XmlAttribute("rowspan")]
        public int RowSpan
        {
            get;
            set;
        }

        [XmlAttribute("colspan")]
        public int ColumnSpan
        {
            get;
            set;
        }

        [XmlIgnore]
        public IEnumerable<IPlacable> Children
        {
            get { return this.ChildElements; }
        }

        #endregion
    }
}
