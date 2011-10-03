using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Malt.Layout.Models
{
    [XmlType("notebook")]
    public sealed class Notebook : Placable
    {
        private static readonly Page[] EmptyPages = new Page[] { };

        public Notebook()
        {
            this.ColumnSpan = 4;
            this.RowSpan = 1;
            this.Height = -1.0F;
            this.Width = -1.0F;
            this.Fill = true;
            this.Pages = EmptyPages;
        }

        #region IContainer 成员

        [XmlElement(ElementName = "page", IsNullable = true, Type = typeof(Page))]
        public Page[] Pages
        {
            get;
            set;
        }

        #endregion

        #region IPlacable 成员

        [XmlAttribute("height")]
        public double Height { get; set; }

        [XmlAttribute("width")]
        public double Width { get; set; }

        #endregion
    }
}
