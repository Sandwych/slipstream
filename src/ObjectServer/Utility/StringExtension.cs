using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ObjectServer.Utility
{
    public static class StringExtension
    {
        public static string SqlEscape(this string value)
        {
            return value.Replace("'", "''");
        }
    }
}
