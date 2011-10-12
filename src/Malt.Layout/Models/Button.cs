using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Malt.Layout.Models
{
    [XmlType("button")]
    public sealed class Button : Placable
    {
        public Button()
        {
            this.Target = "workflow";
            this.States = string.Empty;
            this.Text = string.Empty;
            this.Name = string.Empty;
            this.Confirmation = string.Empty;
            this.Icon = string.Empty;
        }

        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("text")]
        public string Text { get; set; }

        [XmlAttribute("target")]
        public string Target { get; set; }

        [XmlAttribute("confirmation")]
        public string Confirmation { get; set; }

        [XmlAttribute("icon")]
        public string Icon { get; set; }

        [XmlAttribute("states")]
        public string States { get; set; }
    }
}
