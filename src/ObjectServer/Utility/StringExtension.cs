using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ObjectServer.Utility
{
    public static class StringExtension
    {
        private static readonly Regex IdPattern = new Regex(@"^[a-z_][a-z_0-9]*$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        public static string SqlEscape(this string value)
        {
            return value.Replace("'", "''");
        }

        public static bool IsValidSqlIdentifier(this string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentNullException("value");
            }

            return IdPattern.IsMatch(value);
        }
    }
}
