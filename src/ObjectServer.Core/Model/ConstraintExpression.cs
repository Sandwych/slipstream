using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Newtonsoft.Json;

namespace ObjectServer.Model
{
    /// <summary>
    /// 用于表示三元组形式的约束表达式： (field,"=", value)
    /// </summary>
    [Serializable]
    [JsonArray(false)]
    public class ConstraintExpression
    {
        private static readonly HashSet<string> Operators = new HashSet<string>()
        {
             "=", "!=", ">", ">=", "<", "<=", "in", "!in",  "like", "!like", "childof", "!childof"
        };

        public ConstraintExpression(object o)
        {
            if (o == null)
            {
                throw new ArgumentNullException("o");
            }

            var arr = (object[])o;

            if (arr.Length != 3)
            {
                throw new ArgumentOutOfRangeException("o");
            }

            var field = (string)arr[0];
            var opr = (string)arr[1];

            if (string.IsNullOrEmpty(field))
            {
                throw new ArgumentNullException("field");
            }

            if (string.IsNullOrEmpty(opr))
            {
                throw new ArgumentNullException("opr");
            }

            VerifyOperatorAndValue(opr, arr[2]);

            this.Field = field;
            this.Operator = opr;
            this.Value = arr[2];
        }

        public ConstraintExpression(string field, string opr, object value)
        {
            if (string.IsNullOrEmpty(field))
            {
                throw new ArgumentNullException("field");
            }

            if (string.IsNullOrEmpty(opr))
            {
                throw new ArgumentNullException("opr");
            }

            VerifyOperatorAndValue(opr, value);

            this.Field = field;
            this.Operator = opr;
            this.Value = value;
        }

        private static void VerifyOperatorAndValue(string opr, object value)
        {
            if (!Operators.Contains(opr))
            {
                var msg = String.Format("Not supported operator: [{0}]", opr);
                throw new NotSupportedException(msg);
            }
        }

        [JsonProperty("field")]
        public string Field { get; private set; }

        [JsonProperty("operator")]
        public string Operator { get; private set; }

        [JsonProperty("value")]
        public object Value { get; private set; }
    }
}
