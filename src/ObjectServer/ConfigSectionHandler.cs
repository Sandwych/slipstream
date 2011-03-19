using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.XPath;

namespace ObjectServer
{
    public sealed class ConfigSectionHandler : IConfigurationSectionHandler
    {
        public object Create(
             object parent,
             object configContext,
             System.Xml.XmlNode section)
        {
            XmlSerializer xs = new XmlSerializer(typeof(Config));
            using (var reader = new XmlNodeReader(section))
            {
                var obj = xs.Deserialize(reader);
                reader.Close();
                return obj;
            }
        }
    }
}
