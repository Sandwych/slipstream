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

        private static readonly IExpression s_trueExp = new BinaryExpression(
            new ValueExpression(0), ExpressionOperator.EqualOperator, new ValueExpression(0));
        private static readonly List<object[]> EmptyDomain = new List<object[]>();

        private List<string> tables = new List<string>();
        private string mainTable;

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
            this.mainTable = model.TableName;
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
            if (!Operators.Contains(opr))
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
                var exp = ParseSingleDomain(field, opr, value);
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

        private IExpression ParseSingleDomain(string field, string opr, object value)
        {
            var aliasedField = field;
            if (!field.Contains('.'))
            {
                aliasedField = this.mainTable + "." + field;
            }

            IExpression exp = null;
            switch (opr)
            {
                case "=":
                case ">":
                case ">=":
                case "<":
                case "<=":
                    exp = new BinaryExpression(
                        new IdentifierExpression(aliasedField),
                        new ExpressionOperator(opr),
                        new ValueExpression(value));
                    break;

                case "!=":
                    exp = new BinaryExpression(
                        new IdentifierExpression(aliasedField),
                        ExpressionOperator.NotEqualOperator,
                        new ValueExpression(value));
                    break;

                case "like":
                    exp = new BinaryExpression(
                        new IdentifierExpression(aliasedField),
                        ExpressionOperator.LikeOperator,
                        new ValueExpression(value));
                    break;

                case "!like":
                    exp = new BinaryExpression(
                        new IdentifierExpression(aliasedField),
                        ExpressionOperator.NotLikeOperator,
                        new ValueExpression(value));
                    break;

                case "in":
                    exp = new BinaryExpression(
                        new IdentifierExpression(aliasedField),
                        ExpressionOperator.InOperator,
                        new ExpressionGroup((IEnumerable<object>)value));
                    break;

                case "!in":
                    exp = new BinaryExpression(
                        new IdentifierExpression(aliasedField),
                        ExpressionOperator.NotInOperator,
                        new ExpressionGroup((IEnumerable<object>)value));
                    break;

                default:
                    throw new NotSupportedException();

            }

            return exp;
        }


    }
}
