using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Newtonsoft.Json;

namespace ObjectServer.Model
{
    [Serializable]
    [JsonArray(false)]
    public class ConstraintExpression
    {
        public ConstraintExpression()
        {
        }

        public ConstraintExpression(object o)
        {
            if (o == null)
            {
                throw new ArgumentNullException("o");
            }

            var arr = (object[])o;

            this.Field = (string)arr[0];
            this.Operator = (string)arr[1];
            this.Value = arr[2];
        }

        public ConstraintExpression(string field, string opr, object value)
        {
            if (string.IsNullOrEmpty(field))
            {
                throw new ArgumentNullException("field");
            }

            //TODO 检查是否是合法的 opr
            if (string.IsNullOrEmpty(opr))
            {
                throw new ArgumentNullException("opr");
            }

            this.Field = field;
            this.Operator = opr;
            this.Value = value;
        }

        [JsonProperty("field")]
        public string Field { get; set; }

        [JsonProperty("operator")]
        public string Operator { get; set; }

        [JsonProperty("value")]
        public object Value { get; set; }

        public static ConstraintExpression FromTuple(object[] constraint)
        {
            if (constraint == null)
            {
                throw new ArgumentNullException("tuple");
            }
            if (constraint.Length != 3)
            {
                throw new ArgumentException("tuple");
            }

            return new ConstraintExpression((string)constraint[0], (string)constraint[1], constraint[2]);
        }
    }
}
