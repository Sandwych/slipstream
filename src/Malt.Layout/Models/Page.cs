using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Malt.Layout.Models
{
    [XmlType("page")]
    public sealed class Page : IContainer
    {
        public Page()
        {
            this.ColumnCount = 4;
            this.ColumnSpan = 1;
            this.RowSpan = 1;
            this.Fill = false;
            this.Label = string.Empty;
        }

        #region IContainer 成员

        [XmlElement(ElementName = "label", IsNullable = true, Type = typeof(Label))]
        [XmlElement(ElementName = "field", IsNullable = true, Type = typeof(Field))]
        [XmlElement(ElementName = "br", IsNullable = true, Type = typeof(NewLine))]
        [XmlElement(ElementName = "placeholder", IsNullable = true, Type = typeof(PlaceHolder))]
        [XmlElement(ElementName = "hr", IsNullable = true, Type = typeof(HorizontalLine))]
        [XmlElement(ElementName = "notebook", IsNullable = true, Type = typeof(Notebook))]
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
        public int RowSpan { get; set; }

        [XmlAttribute("colspan")]
        public int ColumnSpan { get; set; }

        [XmlAttribute("fill")]
        public bool Fill { get; set; }

        [XmlAttribute("label")]
        public string Label { get; set; }

        [XmlIgnore]
        public IEnumerable<IPlacable> Children
        {
            get { return this.ChildElements; }
        }

        #endregion
    }
}
