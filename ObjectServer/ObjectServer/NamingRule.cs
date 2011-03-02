using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ObjectServer
{
    //TODO: 完善这里的 Regex
    public static class NamingRule
    {
        private static readonly Regex s_serviceNameRegex = 
            new Regex(@"^\w+\.(\w+\.?)+", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private static readonly Regex s_methodNameRegex = 
            new Regex(@"^\w+", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private static readonly Regex s_fieldNameRegex = 
            new Regex(@"^\w+", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        public static bool IsValidServiceName(string name)
        {
            return s_serviceNameRegex.IsMatch(name);
        }

        public static bool IsValidMethodName(string name)
        {
            return s_methodNameRegex.IsMatch(name);
        }

        public static bool IsValidFieldName(string name)
        {
            return s_fieldNameRegex.IsMatch(name);
        }
    }
}
