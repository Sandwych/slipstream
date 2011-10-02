using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace ObjectServer.Data
{
    public enum DatabaseType
    {
        //Mssql,

        [XmlEnum("postgresql")]
        Postgresql,

        [XmlEnum("oracle")]
        Oracle,

        //Sqlite
    }
}
