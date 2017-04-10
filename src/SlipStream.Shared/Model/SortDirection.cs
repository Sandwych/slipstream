using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace SlipStream.Entity
{
    public enum SortDirection
    {
        Ascend,
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

        public static string ToSql(this SortDirection so)
        {
            if (so == SortDirection.Ascend)
            {
                return "ASC";
            }
            else
            {
                return "DESC";
            }
        }
    }
}
