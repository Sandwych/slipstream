using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Sandwych.Layout.Models
{
    [XmlType("label")]
    public sealed class Label : Placable
    {
        [XmlAttribute("text")]
        public string Text { get; set; }


        [XmlAttribute("field")]
        public string Field { get; set; }
    }
}
