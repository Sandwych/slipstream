using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using Newtonsoft.Json;

namespace SlipStream.Entity
{
    /// <summary>
    /// 用于表示三元组形式的约束表达式： (field,"=", value)
    /// </summary>
    [JsonArray(false)]
    public class Criterion
    {
        public const string EqualOperator = "=";
        public const string NotEqualOperator = "!=";
        public const string InOperator = "in";
        public const string NotInOperator = "!in";
        public const string GreaterOperator = ">";
        public const string GreaterEqualOperator = ">=";
        public const string LessOperator = "<";
        public const string LessEqualOperator = "<=";
        public const string LikeOperator = "like";
        public const string NotLikeOperator = "!like";
        public const string ChildOfOperator = "childof";
        public const string NotChildOfOperator = "!childof";

        private static readonly HashSet<string> Operators;
        private static readonly Criterion s_negeativeCriterion;

        static Criterion()
        {
            Operators = new HashSet<string>()
            {
                 "=", "!=", "in", "!in", ">", ">=", "<", "<=", "like", "!like", "childof", "!childof"
            };

            s_negeativeCriterion = new Criterion("_id", "=", (long)0);
        }

        public Criterion(object o)
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

            this.Field = field.Trim().ToLowerInvariant();
            this.Operator = opr.Trim().ToLowerInvariant();
            this.Value = arr[2];
        }

        public Criterion(string field, string opr, object value)
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

        public object[] ToPlainCriterion()
        {
            Debug.Assert(!string.IsNullOrEmpty(this.Operator));

            return new object[] { this.Field, this.Operator, this.Value };
        }

        public static Criterion NegeativeCriterion { get { return s_negeativeCriterion; } }
    }
}
