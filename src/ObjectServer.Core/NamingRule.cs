using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ObjectServer
{
    /// <summary>
    /// 系统的命名规则
    /// </summary>
    internal static class NamingRule
    {
        public static readonly Regex ResourcePattern =
            new Regex(@"^([a-z_][a-z_0-9]*)\.([a-z_][a-z_0-9]*)$",
                RegexOptions.Compiled | RegexOptions.CultureInvariant);

        public static readonly Regex SqlNamePattern =
            new Regex(@"^[a-z_][a-z_0-9]*$",
                RegexOptions.Compiled | RegexOptions.CultureInvariant);

        public static readonly Regex IdentifierPattern =
            new Regex(@"^[A-Za-z_][A-Za-z_0-9]*$",
                RegexOptions.Compiled | RegexOptions.CultureInvariant);

        public static readonly Regex FieldPattern =
            new Regex(@"^[a-z_][a-z_0-9]*$",
                RegexOptions.Compiled | RegexOptions.CultureInvariant);

        public static bool IsValidResourceName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }

            return ResourcePattern.IsMatch(name);
        }

        public static bool IsValidMethodName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }

            return IdentifierPattern.IsMatch(name);
        }

        public static bool IsValidFieldName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }

            return FieldPattern.IsMatch(name);
        }

        public static bool IsValidSqlName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }

            return SqlNamePattern.IsMatch(name);
        }
    }
}
