using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace ObjectServer.Sql
{
    public enum SortDirection
    {
        [EnumMember(Value = "asc")]
        Asc,
        [EnumMember(Value = "desc")]
        Desc
    }

    public static class SortDirectionParser
    {
        public static SortDirection Parser(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentNullException("value");
            }

            value = value.ToLowerInvariant();

            switch (value)
            {
                case "asc":
                    return SortDirection.Asc;

                case "desc":
                    return SortDirection.Desc;

                default:
                    throw new NotSupportedException();
            }
        }

        public static string ToUpperString(this SortDirection so)
        {
            return so.ToString().ToUpper();
        }
    }
}
