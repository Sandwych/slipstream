using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace ObjectServer.Sql
{
    public enum SortDirection
    {
        [EnumMember(Value = "ASC")]
        Ascend,
        [EnumMember(Value = "DESC")]
        Descend
    }

    public static class SortDirectionParser
    {
        public static SortDirection Parser(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentNullException("value");
            }

            value = value.ToUpperInvariant();

            switch (value)
            {
                case "ASC":
                    return SortDirection.Ascend;

                case "DESC":
                    return SortDirection.Descend;

                default:
                    throw new NotSupportedException();
            }
        }

        public static string ToUpperString(this SortDirection so)
        {
            return so.ToString();
        }
    }
}
