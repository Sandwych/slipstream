using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Sandwych.Layout.Models
{
    [XmlType("input")]
    public class Input : Placable
    {
        [XmlAttribute("field")]
        public string Field { get; set; }

    }
}
