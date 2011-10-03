using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Malt.Layout.Models
{
    [XmlType("page")]
    public sealed class Page : AbstractContainer
    {
        public Page()
        {
            this.Label = string.Empty;
        }


        [XmlAttribute("label")]
        public string Label { get; set; }

    }
}
