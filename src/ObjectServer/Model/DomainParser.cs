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
            "like", "!like", "childof", "!childof"
        };

        private static readonly IExpression s_trueExp = new BinaryExpression(
            new ValueExpression(0), ExpressionOperator.EqualOperator, new ValueExpression(0));
        private static readonly List<DomainInfo> EmptyDomain = new List<DomainInfo>();

        private List<AliasExpression> tables = new List<AliasExpression>();
        private List<IExpression> joinRestrictions = new List<IExpression>();
        private string mainTable;

        IModel model;
        IServiceScope serviceScope;
        List<DomainInfo> internalDomain = new List<DomainInfo>();
        int aliasIndexCount = 0;

        public DomainParser(IServiceScope scope, IModel model, IEnumerable<object> domain)
        {
            Debug.Assert(scope != null);
            Debug.Assert(model != null);

            //TODO 过滤掉不能处理的字段，比如函数字段等

            if (domain == null || domain.Count() <= 0)
            {
                this.internalDomain = EmptyDomain;
            }
            else
            {
                foreach (object[] o in domain)
                {
                    this.internalDomain.Add(DomainInfo.FromTuple(o));
                }
            }

            this.serviceScope = scope;
            this.model = model;
            this.mainTable = model.TableName;
            this.tables.Add(new AliasExpression(model.TableName));

            if (model.Inheritances.Count > 0)
            {
                this.AddInheritedTables(scope, model);
            }
        }

        private void AddInheritedTables(IServiceScope scope, IModel model)
        {
            Debug.Assert(model.Inheritances.Count > 0);

            //TODO: 这里检查过滤规则等，处理查询非表中字段等
            //TODO: 自动添加 active 字段
            //TODO 处理 childof 等复杂查询
            //继承查询的策略很简单，直接把基类表连接到查询里
            //如果有重复的字段，就以子类的字段为准
            var usedInheritances = new List<InheritanceInfo>();
            for (int i = 0; i < this.internalDomain.Count; i++)
            {
                var d = this.internalDomain[i];
                string tableName = null;
                var metaField = model.Fields[d.Field];

                if (AbstractTableModel.SystemReadonlyFields.Contains(d.Field))
                {
                    tableName = this.mainTable;
                }
                else
                {
                    var tableNames =
                        from inherit in model.Inheritances
                        let bm = (AbstractTableModel)scope.GetResource(inherit.BaseModel)
                        where bm.Fields.ContainsKey(d.Field)
                        select inherit;
                    var ii = tableNames.Single();
                    usedInheritances.Add(ii);
                    tableName = ((AbstractTableModel)scope.GetResource(ii.BaseModel)).TableName;
                    this.internalDomain[i] = new DomainInfo(tableName + '.' + d.Field, d.Operator, d.Value);
                }

                foreach (var inheritInfo in usedInheritances)
                {
                    var baseModel = (AbstractTableModel)scope.GetResource(inheritInfo.BaseModel);
                    this.tables.Add(new AliasExpression(baseModel.TableName));
                    var joinExp = new BinaryExpression(
                        new IdentifierExpression(mainTable + '.' + inheritInfo.RelatedField),
                        ExpressionOperator.EqualOperator,
                        new IdentifierExpression(baseModel.TableName + ".id"));
                    this.joinRestrictions.Add(joinExp);
                }
            }
        }

        public IList<AliasExpression> Tables { get { return this.tables; } }

        /*
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

            this.internalDomain.Add(exp);
        }*/

        public bool ContainsField(string field)
        {
            return this.internalDomain.Exists(exp => exp.Field == field);
        }

        public IExpression Parse()
        {
            if (this.internalDomain == null || this.internalDomain.Count == 0)
            {
                return (IExpression)s_trueExp.Clone();
            }

            var expressions = new List<IExpression>(this.joinRestrictions.Count + this.internalDomain.Count + 1);
            expressions.AddRange(this.joinRestrictions);

            foreach (var domainItem in this.internalDomain)
            {
                var exp = ParseSingleDomain(domainItem);
                var bracketExp = new BracketedExpression(exp);
                expressions.Add(bracketExp);
            }
            var whereExp = JoinExpressionsByAnd(expressions);

            return whereExp;
        }

        private static IExpression JoinExpressionsByAnd(IList<IExpression> expressions)
        {
            Debug.Assert(expressions != null);
            Debug.Assert(expressions.Count > 0);

            IExpression expTop;
            int expCount = expressions.Count;

            if (expressions.Count % 2 != 0)
            {
                //为了方便 AND 连接起见，在奇数个表达式最后加上总是 0 = 0 的表达式
                expTop = s_trueExp;
                expCount++;
            }
            else
            {
                expTop = expressions.Last();
            }

            for (int i = expCount - 2; i >= 0; i--)
            {
                var rhs = expTop;
                var andExp = new BinaryExpression(expressions[i], ExpressionOperator.AndOperator, rhs);
                expTop = andExp;
            }
            return expTop;
        }

        private IExpression ParseSingleDomain(DomainInfo domain)
        {
            var aliasedField = domain.Field;
            if (!domain.Field.Contains('.'))
            {
                aliasedField = this.mainTable + "." + domain.Field;
            }

            IExpression exp = null;
            switch (domain.Operator)
            {
                case "=":
                case ">":
                case ">=":
                case "<":
                case "<=":
                    exp = new BinaryExpression(
                        new IdentifierExpression(aliasedField),
                        new ExpressionOperator(domain.Operator),
                        new ValueExpression(domain.Value));
                    break;

                case "!=":
                    exp = new BinaryExpression(
                        new IdentifierExpression(aliasedField),
                        ExpressionOperator.NotEqualOperator,
                        new ValueExpression(domain.Value));
                    break;

                case "like":
                    exp = new BinaryExpression(
                        new IdentifierExpression(aliasedField),
                        ExpressionOperator.LikeOperator,
                        new ValueExpression(domain.Value));
                    break;

                case "!like":
                    exp = new BinaryExpression(
                        new IdentifierExpression(aliasedField),
                        ExpressionOperator.NotLikeOperator,
                        new ValueExpression(domain.Value));
                    break;

                case "in":
                    exp = new BinaryExpression(
                        new IdentifierExpression(aliasedField),
                        ExpressionOperator.InOperator,
                        new ExpressionGroup((IEnumerable<object>)domain.Value));
                    break;

                case "!in":
                    exp = new BinaryExpression(
                        new IdentifierExpression(aliasedField),
                        ExpressionOperator.NotInOperator,
                        new ExpressionGroup((IEnumerable<object>)domain.Value));
                    break;

                case "childof":
                    exp = this.ParseChildOfOperator(domain.Field, domain.Value);
                    break;


                case "!childof":
                    throw new NotImplementedException();

                default:
                    throw new NotSupportedException();

            }

            return exp;
        }

        private IExpression ParseChildOfOperator(string field, object value)
        {
            //TODO 处理继承字段
            var joinTableName = string.Empty;
            if (field == "id")
            {
                joinTableName = mainTable;
            }
            else
            {
                var fieldInfo = this.model.Fields[field];
                var joinModel = (IModel)this.serviceScope.GetResource(fieldInfo.Relation);
                joinTableName = joinModel.TableName;
            }
            //TODO 确认 many2one 类型字段
            var aliasIndex = this.aliasIndexCount++;
            var parentAliasName = string.Format("_{0}_parent_{1}", joinTableName, aliasIndex);
            var childAliasName = string.Format("_{0}_child_{1}", joinTableName, aliasIndex);
            this.tables.Add(new AliasExpression(joinTableName, parentAliasName));
            this.tables.Add(new AliasExpression(joinTableName, childAliasName));
            /* 生成的 SQL 形如：
             * SELECT mainTable.id 
             * FROM mainTable, category _category_parent_0, category AS _category_child_0
             * WHERE _category_child_0.id = mainTable.field AND
             *     _category_parent_0.id = {value} AND
             *     _category_child_0._left > _category_parent_0._left AND
             *     _category_child_0._left < _category_parent_0._right AND ...
             * 
             * */
            var exps = new IExpression[]
                    {
                        new BinaryExpression(new IdentifierExpression(childAliasName + ".id"), 
                            ExpressionOperator.EqualOperator,  
                            new IdentifierExpression(this.mainTable + "." + field)),

                        new BinaryExpression(parentAliasName + ".id", "=", value),

                        new BinaryExpression(new IdentifierExpression(childAliasName + "._left"),
                            ExpressionOperator.GreaterOperator,
                            new IdentifierExpression(parentAliasName + "._left")),

                        new BinaryExpression(new IdentifierExpression(childAliasName + "._left"), 
                            ExpressionOperator.LessOperator, 
                            new IdentifierExpression(parentAliasName + "._right")),
                    };

            var exp = JoinExpressionsByAnd(exps);
            return exp;
        }


    }
}
