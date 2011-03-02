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
                    var userValues = (IEnumerable<object>)value;
                    var values = new ExpressionGroup(userValues);
                    return new BinaryExpression(
                        columnExp, ExpressionOperator.InOperator, values); 
                }},
                
                {"!in", (fieldName, value) => {
                    var columnExp = new IdentifierExpression(fieldName);
                    var userValues = (IEnumerable<object>)value;
                    var values = new ExpressionGroup(userValues);
                    return new BinaryExpression(
                        columnExp, ExpressionOperator.NotInOperator, values); 
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

        public DomainParser(IModel model, IEnumerable<object> domain)
        {
            if (domain == null || domain.Count() <= 0)
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

            var opr = (string)exp[1];
            if (!s_oprWhereProcessorMapping.Keys.Contains(opr))
            {
                throw new NotSupportedException("Not supported domain operator: " + opr);
            }

            this.domain.Add(exp);
        }

        public bool ContainsField(string field)
        {
            return this.domain.Exists(exp => (string)exp[0] == field);
        }

        public IExpression ToExpressionTree()
        {
            if (this.domain == null || this.domain.Count == 0)
            {
                return (IExpression)s_trueExp.Clone();
            }

            var expressions = new List<IExpression>(this.domain.Count + 1);

            foreach (var domainItem in this.domain)
            {
                var field = (string)domainItem[0];
                var opr = (string)domainItem[1];
                var value = domainItem[2];

                //TODO: 考虑单元运算符
                var expFactory = s_oprWhereProcessorMapping[opr];
                var exp = expFactory(field, value);
                var bracketExp = new BracketedExpression(exp);
                expressions.Add(bracketExp);
            }

            if (expressions.Count % 2 != 0)
            {
                //为了方便 AND 连接起见，在奇数个表达式最后加上总是 True 的单目表达式
                expressions.Add(s_trueExp);
            }

            int andExpCount = expressions.Count / 2;

            var whereExps = new IExpression[andExpCount];
            for (int i = 0; i < andExpCount; ++i)
            {
                var andExp = new BinaryExpression(
                    expressions[i * 2], ExpressionOperator.AndOperator, expressions[i * 2 + 1]);
                whereExps[i] = andExp;
            }

            return whereExps[0];
        }


    }
}
