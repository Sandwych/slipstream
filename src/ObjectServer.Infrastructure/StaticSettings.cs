using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace ObjectServer
{
    public static class StaticSettings
    {
        public const string CoreModuleName = "core";

        public const string ModuleMetaDataFileName = "module.xml";

        public const string LogFileName = "objectserver.log";

        public const string LogPattern =
            "[%date %-5level]: %message%newline";

        public static Version Version
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetName().Version;
            }
        }
    }
}
