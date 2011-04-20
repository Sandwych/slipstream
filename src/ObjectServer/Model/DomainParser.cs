using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

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

        private List<AliasExpression> tables = new List<AliasExpression>();
        private List<IExpression> joinRestrictions = new List<IExpression>();
        private string mainTable;

        IModel model;
        IServiceScope serviceScope;
        List<object[]> domain = new List<object[]>();

        public DomainParser(IServiceScope scope, IModel model, IEnumerable<object> domain)
        {
            Debug.Assert(scope != null);
            Debug.Assert(model != null);

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

            this.serviceScope = scope;
            this.model = model;
            this.mainTable = model.TableName;
            this.tables.Add(new AliasExpression(model.TableName));
            this.AddInheritedTables(scope, model);
        }

        private void AddInheritedTables(IServiceScope scope, IModel model)
        {

            //TODO: 这里检查过滤规则等，处理查询非表中字段等
            //TODO: 自动添加 active 字段
            //TODO 处理 childof 等复杂查询
            //继承查询的策略很简单，直接把基类表连接到查询里
            //如果有重复的字段，就以子类的字段为准
            var tables = new List<AliasExpression>();
            tables.Add(new AliasExpression(mainTable));
            if (model.Inheritances.Count > 0)
            {
                foreach (var d in this.domain)
                {
                    string tableName = null;
                    var e = (object[])d;
                    var fieldName = (string)e[0];
                    var metaField = model.Fields[fieldName];

                    if (AbstractTableModel.SystemReadonlyFields.Contains(fieldName))
                    {
                        tableName = model.TableName;
                    }
                    else
                    {
                        var tableNames =
                            from i in model.Inheritances
                            let bm = (AbstractTableModel)scope.GetResource(i.BaseModel)
                            where bm.Fields.ContainsKey(fieldName)
                            select bm.TableName;
                        tableName = tableNames.Single();
                    }

                    e[0] = tableName + '.' + fieldName;
                }

                foreach (var inheritInfo in model.Inheritances)
                {
                    var baseModel = (AbstractTableModel)scope.GetResource(inheritInfo.BaseModel);
                    this.tables.Add(new AliasExpression(baseModel.TableName));
                    this.joinRestrictions.Add(new BinaryExpression(
                        new IdentifierExpression(mainTable + '.' + inheritInfo.RelatedField),
                        ExpressionOperator.EqualOperator,
                        new IdentifierExpression(baseModel.TableName + ".id")));
                }
            }
        }

        public IList<AliasExpression> Tables { get { return this.tables; } }

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

            var expressions = new List<IExpression>(this.joinRestrictions.Count + this.domain.Count + 1);
            expressions.AddRange(this.joinRestrictions);

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
