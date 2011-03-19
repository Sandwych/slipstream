using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.Model
{
    public enum SearchOrder
    {
        Asc,
        Desc
    }

    public static class SearchOrderParser
    {
        public static SearchOrder Parser(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentNullException("value");
            }

            value = value.ToUpperInvariant();

            switch (value)
            {
                case "ASC":
                    return SearchOrder.Asc;

                case "DESC":
                    return SearchOrder.Desc;

                default:
                    throw new NotSupportedException();
            }
        }
    }
}
