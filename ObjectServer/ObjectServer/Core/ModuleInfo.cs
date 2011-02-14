using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace ObjectServer.Core
{
    /// <summary>
    /// DTO class for Module
    /// </summary>
    [Serializable]
    [XmlRoot("module")]
    public sealed class ModuleInfo
    {
        [XmlElement("name")]
        public string Name { get; set; }

        [XmlElement("label")]
        public string Label { get; set; }

        [XmlElement("description")]
        public string Description { get; set; }

        [XmlArray("source-files")]
        [XmlArrayItem("file", typeof(string))]
        public string[] SourceFiles { get; set; }

        [XmlArray("data-file")]
        public string[] DataFiles { get; set; }

    }
}
