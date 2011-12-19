using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Newtonsoft.Json;

namespace ObjectServer.Model
{

#if !SILVERLIGHT
    [Serializable]
#endif
    [JsonArray]
    public class OrderExpression
    {
        private static readonly OrderExpression[] DefaultOrders =
            new OrderExpression[] { new OrderExpression("_id", SortDirection.Ascend) };

        public OrderExpression(string field, SortDirection so)
        {
            if (string.IsNullOrEmpty(field))
            {
                throw new ArgumentNullException("field");
            }

            this.Field = field;
            this.Order = so;
        }

        [JsonProperty("field")]
        public string Field { get; set; }

        [JsonProperty("order")]
        public SortDirection Order { get; set; }

        public static OrderExpression[] GetDefaultOrders()
        {
            return DefaultOrders;
        }
    }
}
