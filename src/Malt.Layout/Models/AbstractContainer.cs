using System;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace Malt.Layout.Models
{
    public abstract class AbstractContainer : IContainer
    {
        private static readonly Placable[] EmptyChildElements = new Placable[] { };

        public AbstractContainer()
        {
            this.ColumnCount = 4;
            this.ColumnSpan = 4;
            this.RowSpan = 1;
            this.Fill = true;
            this.ChildElements = EmptyChildElements;
        }

        [XmlElement(ElementName = "label", IsNullable = true, Type = typeof(Label))]
        [XmlElement(ElementName = "input", IsNullable = true, Type = typeof(Input))]
        [XmlElement(ElementName = "button", IsNullable = true, Type = typeof(Button))]
        [XmlElement(ElementName = "br", IsNullable = true, Type = typeof(NewLine))]
        [XmlElement(ElementName = "placeholder", IsNullable = true, Type = typeof(PlaceHolder))]
        [XmlElement(ElementName = "hr", IsNullable = true, Type = typeof(HorizontalLine))]
        [XmlElement(ElementName = "notebook", IsNullable = true, Type = typeof(Notebook))]
        public Placable[] ChildElements
        {
            get;
            set;
        }

        #region IContainer 成员
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

        [XmlIgnore]
        public IEnumerable<IPlacable> Children
        {
            get { return this.ChildElements; }
        }

        #endregion
    }
}
