using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.Model
{
    [Serializable]
    public struct OrderExpression
    {
        private static readonly OrderExpression[] DefaultOrders = new OrderExpression[] { new OrderExpression("id", SortDirection.Asc) };

        public OrderExpression(string field, SortDirection so)
            : this()
        {
            if (string.IsNullOrEmpty(field))
            {
                throw new ArgumentNullException("field");
            }

            this.Field = field;
            this.Order = so;
        }

        public string Field { get; private set; }
        public SortDirection Order { get; private set; }

        public static OrderExpression[] GetDefaultOrders()
        {
            return DefaultOrders;
        }
    }
}
