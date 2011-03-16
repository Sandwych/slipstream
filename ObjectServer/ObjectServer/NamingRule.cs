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
        private static readonly Regex s_resourceNameRegex =
            new Regex(@"([a-z_][a-z_0-9]*)\.([a-z_][a-z_0-9]*)", 
                RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private static readonly Regex s_methodNameRegex = 
            new Regex(@"([A-Za-z_][A-Za-z_0-9]*)", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private static readonly Regex s_fieldNameRegex = 
            new Regex(@"([a-z_][a-z_0-9]*)", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        public static bool IsValidResourceName(string name)
        {
            return s_resourceNameRegex.IsMatch(name);
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
