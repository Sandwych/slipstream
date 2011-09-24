using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Malt.Layout.Models
{
    [XmlType("field")]
    public class Field : Placable
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

    }
}
