using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.Model
{
    internal sealed class DomainParser
    {
        public static readonly string[] Operators = new string[]
        {
            "=", "!=", ">", ">=", "<", "<=", "in", "not in", 
            "like", "not like", "childof"
        };

        private static readonly List<object[]> EmptyDomain = new List<object[]>();

        ModelBase model;
        List<object[]> expressions = new List<object[]>();

        public DomainParser(ModelBase model, IList<object[]> domain)
        {
            if (domain == null)
            {
                this.expressions = EmptyDomain;
            }
            else
            {
                this.expressions.AddRange(domain);
            }

            this.model = model;
        }

        public DomainParser(ModelBase model)
            : this(model, null)
        {
        }

        public void AddExpression(object[] exp)
        {
            if (exp.Length != 3)
            {
                throw new ArgumentException("exp");
            }

            this.expressions.Add(exp);
        }

        public string ToSql()
        {
            var sqlBuilder = new StringBuilder();
            var first = true;
            foreach (var exp in this.expressions)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    sqlBuilder.Append(" and ");
                }

                var opr = (string)exp[1];
                var field = (string)exp[0];
                var value = exp[2];

                sqlBuilder.Append(" (");
                sqlBuilder.Append(field);
                sqlBuilder.Append(' ');
                sqlBuilder.Append(opr);
                sqlBuilder.Append(' ');
                AddValue(sqlBuilder, field, value);
                sqlBuilder.Append(") ");
            }

            return sqlBuilder.ToString();
        }

        private void AddValue(StringBuilder sb, string field, object value)
        {
            var fieldType = this.model.DeclaredFields.Single(f => f.Name == field).Type;

            switch (fieldType)
            {
                case FieldType.Text:
                case FieldType.Chars:
                    var str = (string)value;
                    str.Replace("'", "");
                    sb.Append("'");
                    sb.Append(str);
                    sb.Append("'");
                    break;

                case FieldType.ManyToOne:
                case FieldType.BigInteger:
                case FieldType.Integer:
                case FieldType.Float:
                case FieldType.Money:
                    var numStr = value.ToString().Replace("'", "");
                    sb.Append(numStr);
                    break;

                default:
                    throw new NotSupportedException();

            }
        }

        private void AddOperator(StringBuilder sb, string field, string opr)
        {
            throw new NotImplementedException();
        }
    }
}
