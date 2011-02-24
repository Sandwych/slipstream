using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ObjectServer.SqlTree;

namespace ObjectServer.Model
{
    internal sealed class DomainParser
    {
        public static readonly string[] Operators = new string[]
        {
            "=", "!=", ">", ">=", "<", "<=", "in", "not in", 
            "like", "not like", "childof"
        };

        private static readonly IExpression s_trueExp = new ValueExpression(true);

        private static readonly List<object[]> EmptyDomain = new List<object[]>();

        IModel model;
        List<object[]> domain = new List<object[]>();

        public DomainParser(IModel model, IList<object[]> domain)
        {
            if (domain == null)
            {
                this.domain = EmptyDomain;
            }
            else
            {
                this.domain.AddRange(domain);
            }

            this.model = model;
        }

        public DomainParser(IModel model)
            : this(model, null)
        {
        }

        public void AddExpression(object[] exp)
        {
            if (exp.Length != 3)
            {
                throw new ArgumentException("the parameter 'exp' must have 3 elements", "exp");
            }

            this.domain.Add(exp);
        }

        public bool ContainsField(string field)
        {
            return this.domain.Exists(exp => (string)exp[0] == field);
        }

        public string ToSql()
        {
            var sqlBuilder = new StringBuilder();
            var first = true;
            foreach (var exp in this.domain)
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


        public IExpression ToExpressionTree()
        {
            var expressions = new List<IExpression>(this.domain.Count + 1);

            foreach (var domainItem in this.domain)
            {
                var opr = (string)domainItem[1];
                var field = (string)domainItem[0];
                var value = domainItem[2];

                //考虑单元运算符
                var exp = new BinaryExpression(field, opr, value);
                expressions.Add(exp);
            }
            expressions.Add(s_trueExp);

            var whereExps = new List<IExpression>(this.domain.Count);
            for (int i = 0; i < expressions.Count; i += 2)
            {
                var andExp = new BinaryExpression(expressions[i], ExpressionOperator.AndOperator, expressions[i + 1]);
                whereExps.Add(andExp);
            }

            return whereExps[0];
        }

        private void AddValue(StringBuilder sb, string field, object value)
        {
            var fieldType = this.model.DefinedFields[field].Type;

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
                case FieldType.Boolean:
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
