using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;

using ObjectServer.SqlTree;

namespace ObjectServer.Model
{
    internal sealed class DomainParser
    {
        public static readonly string[] Operators = new string[]
        {
            "=", "!=", ">", ">=", "<", "<=", "in", "!in", 
            "like", "!like", "childof"
        };

        public static readonly Dictionary<string, Func<string, object, IExpression>> s_oprWhereProcessorMapping
            = new Dictionary<string, Func<string, object, IExpression>>()
            {
                {"=", (fieldName, value) => {
                    return new BinaryExpression(fieldName, "=", value); 
                }},

                {"!=", (fieldName, value) => {
                    return new BinaryExpression(fieldName, "<>", value); 
                }},

                {">", (fieldName, value) => {
                    return new BinaryExpression(fieldName, ">", value); 
                }},

                {"<", (fieldName, value) => {
                    return new BinaryExpression(fieldName, "<", value); 
                }},

                {">=", (fieldName, value) => {
                    return new BinaryExpression(fieldName, ">=", value); 
                }},

                {"<=", (fieldName, value) => {
                    return new BinaryExpression(fieldName, "<=", value); 
                }},

                {"in", (fieldName, value) => {
                    var columnExp = new IdentifierExpression(fieldName);
                    var userValues = (IEnumerable)value;
                    var values = new ExpressionGroup(userValues);
                    return new InExpression(columnExp, values); 
                }},
                
                {"!in", (fieldName, value) => {
                    return new BinaryExpression(fieldName, "NOT IN", value); 
                }},

                {"like", (fieldName, value) => {
                    return new BinaryExpression(fieldName, "LIKE", value); 
                }},

                {"!like", (fieldName, value) => {
                    return new BinaryExpression(fieldName, "NOT LIKE", value); 
                }},

                /*
                {"childof", (fieldName, value) => {
                    var expLeft = new BinaryExpression("_left", ">", 
                    return new BinaryExpression(fieldName, "NOT LIKE", value); 
                }},
                 */

                //TODO: childof
                //select * from Nodes where Left > n.Left and Left < n.Right

            };

        private static readonly IExpression s_trueExp = new ValueExpression(true);

        private static readonly List<object[]> EmptyDomain = new List<object[]>();

        IModel model;
        List<object[]> domain = new List<object[]>();

        public DomainParser(IModel model, IEnumerable domain)
        {
            if (domain == null)
            {
                this.domain = EmptyDomain;
            }
            else
            {
                foreach (object[] o in domain)
                {
                    this.domain.Add(o);
                }
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

        public IExpression ToExpressionTree()
        {
            var expressions = new List<IExpression>(this.domain.Count + 1);

            foreach (var domainItem in this.domain)
            {
                var field = (string)domainItem[0];
                var opr = (string)domainItem[1];
                var value = domainItem[2];

                //考虑单元运算符
                var expFactory = s_oprWhereProcessorMapping[opr];
                var exp = expFactory(field, value);
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


    }
}
