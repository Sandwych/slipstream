using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.Model
{
    [Serializable]
    public struct OrderInfo
    {
        private static readonly OrderInfo[] DefaultOrders = new OrderInfo[] { new OrderInfo("id", SearchOrder.Asc) };

        public OrderInfo(string field, SearchOrder so)
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
        public SearchOrder Order { get; private set; }

        public static OrderInfo[] GetDefaultOrders()
        {
            return DefaultOrders;
        }
    }
}
