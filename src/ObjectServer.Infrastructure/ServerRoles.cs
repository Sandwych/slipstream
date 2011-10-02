using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace ObjectServer
{
    public enum ServerRoles
    {
        [XmlEnum("standalone")]
        Standalone,

        [XmlEnum("worker")]
        Worker,

        [XmlEnum("httpd")]
        HttpServer
    }
}
