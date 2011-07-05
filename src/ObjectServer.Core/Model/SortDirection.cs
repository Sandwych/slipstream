using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.Model
{
    public enum SortDirection
    {
        Asc,
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

            value = value.ToUpperInvariant();

            switch (value)
            {
                case "ASC":
                    return SortDirection.Asc;

                case "DESC":
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
