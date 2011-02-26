using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace ObjectServer
{
    internal static class StaticSettings
    {
        public const string CoreModuleName = "core";
        public const string ModuleMetaDataFileName = "module.js";

        public static Version Version
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetName().Version;
            }
        }
    }
}
