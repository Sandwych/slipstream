using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Sandwych.Layout.Models
{
    [XmlType("form")]
    public class Form : AbstractContainer
    {

        public Form()
            : base()
        {

        }

        [XmlAttribute("label")]
        public string Label { get; set; }

    }
}
