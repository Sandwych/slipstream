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
        private sealed class InnerJoinInfo
        {
            public InnerJoinInfo(string t, string a)
            {
                this.Table = t;
                this.Alias = a;
            }

            public string Table { get; private set; }
            public string Alias { get; private set; }
        }

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
        private List<InnerJoinInfo> innerJoins = new List<InnerJoinInfo>(4);
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


            this.serviceScope = scope;
            this.model = model;
            this.mainTable = model.TableName;
            this.tables.Add(new AliasExpression(model.TableName));

            this.Parse(domain);

            if (model.Inheritances.Count > 0)
            {
                this.AddInheritedTables(scope, model);
            }
        }

        private void Parse(IEnumerable<object> domain)
        {

            foreach (object[] o in domain)
            {
                string aliasedField;
                var di = DomainInfo.FromTuple(o);
                if (!di.Field.Contains('.'))
                {
                    aliasedField = this.mainTable + "." + di.Field;
                }
                else
                {
                    var fields = di.Field.Split('.');
                    if (fields.Length == 2)
                    {
                        var selfField = fields[0];
                        var externalField = fields[1];

                        var fieldInfo = this.model.Fields[selfField];
                        if (fieldInfo.Type == FieldType.ManyToOne)
                        {
                            var joinModel = (IModelDescriptor)this.serviceScope.GetResource(fieldInfo.Relation);
                            var joinTable = joinModel.TableName;
                            var joinAlias = "_t" + this.tables.Count.ToString();
                            this.tables.Add(new AliasExpression(joinTable, joinAlias));
                            this.joinRestrictions.Add(new BinaryExpression(
                                new IdentifierExpression(this.mainTable + '.' + selfField),
                                ExpressionOperator.EqualOperator,
                                new IdentifierExpression(joinAlias + ".id")));
                            aliasedField = joinAlias + '.' + externalField;
                        }
                    }
                }
                this.internalDomain.Add(DomainInfo.FromTuple(o));
            }
        }

        private string PutInnerJoin(string table, string relatedField)
        {
            var joinInfo = this.innerJoins.SingleOrDefault(j => j.Table == table);

            string alias;
            if (joinInfo == null)
            {
                alias = "_t" + this.innerJoins.Count.ToString();
                this.innerJoins.Add(new InnerJoinInfo(table, alias));
                this.tables.Add(new AliasExpression(table, alias));
                this.joinRestrictions.Add(new BinaryExpression(
                    new IdentifierExpression(alias + ".id"),
                    ExpressionOperator.EqualOperator,
                    new IdentifierExpression(this.mainTable + "." + relatedField)));
            }
            else
            {
                alias = joinInfo.Alias;
            }
            return alias;
        }

        private InnerJoinInfo GetInnerJoin(string table)
        {
            return this.innerJoins.SingleOrDefault(j => j.Table == table);
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
                        let bm = (IModelDescriptor)scope.GetResource(inherit.BaseModel)
                        where bm.Fields.ContainsKey(d.Field)
                        select inherit;
                    var ii = tableNames.Single();
                    usedInheritances.Add(ii);
                    tableName = ((IModelDescriptor)scope.GetResource(ii.BaseModel)).TableName;
                    var baseAlias = this.PutInnerJoin(tableName, ii.RelatedField);
                    this.internalDomain[i] = new DomainInfo(
                        baseAlias + '.' + d.Field, d.Operator, d.Value);
                }

                foreach (var inheritInfo in usedInheritances)
                {
                    var baseModel = (IModelDescriptor)scope.GetResource(inheritInfo.BaseModel);
                    this.PutInnerJoin(baseModel.TableName, inheritInfo.RelatedField);
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

        public IExpression ToExpression()
        {
            if (this.internalDomain == null || this.internalDomain.Count == 0)
            {
                return (IExpression)s_trueExp.Clone();
            }

            var expressions = new List<IExpression>(this.joinRestrictions.Count + this.internalDomain.Count + 1);

            foreach (var domainItem in this.internalDomain)
            {
                var exps = ParseLeafDomain(domainItem);
                var exp = JoinExpressionsByAnd(exps);
                var bracketExp = new BracketedExpression(exp);
                expressions.Add(bracketExp);
            }
            expressions.AddRange(this.joinRestrictions);
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

        private IList<IExpression> ParseLeafDomain(DomainInfo domain)
        {
            var exps = new List<IExpression>();
            var aliasedField = domain.Field;


            switch (domain.Operator)
            {
                case "=":
                case ">":
                case ">=":
                case "<":
                case "<=":
                    exps.Add(new BinaryExpression(
                        new IdentifierExpression(aliasedField),
                        new ExpressionOperator(domain.Operator),
                        new ValueExpression(domain.Value)));
                    break;

                case "!=":
                    exps.Add(new BinaryExpression(
                        new IdentifierExpression(aliasedField),
                        ExpressionOperator.NotEqualOperator,
                        new ValueExpression(domain.Value)));
                    break;

                case "like":
                    exps.Add(new BinaryExpression(
                        new IdentifierExpression(aliasedField),
                        ExpressionOperator.LikeOperator,
                        new ValueExpression(domain.Value)));
                    break;

                case "!like":
                    exps.Add(new BinaryExpression(
                        new IdentifierExpression(aliasedField),
                        ExpressionOperator.NotLikeOperator,
                        new ValueExpression(domain.Value)));
                    break;

                case "in":
                    exps.Add(new BinaryExpression(
                        new IdentifierExpression(aliasedField),
                        ExpressionOperator.InOperator,
                        new ExpressionGroup((IEnumerable<object>)domain.Value)));
                    break;

                case "!in":
                    exps.Add(new BinaryExpression(
                        new IdentifierExpression(aliasedField),
                        ExpressionOperator.NotInOperator,
                        new ExpressionGroup((IEnumerable<object>)domain.Value)));
                    break;

                case "childof":
                    exps.AddRange(this.ParseChildOfOperator(domain.Field, domain.Value));
                    break;


                case "!childof":
                    throw new NotImplementedException();

                default:
                    throw new NotSupportedException();

            }

            return exps;
        }

        private IList<IExpression> ParseChildOfOperator(string field, object value)
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
            this.tables.Add(new AliasExpression(joinTableName, parentAliasName));
            var childAliasName = this.PutInnerJoin(joinTableName, field);

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
                        /*
                        new BinaryExpression(new IdentifierExpression(childAliasName + ".id"), 
                            ExpressionOperator.EqualOperator,  
                            new IdentifierExpression(this.mainTable + "." + field)),
                        */

                        new BinaryExpression(parentAliasName + ".id", "=", value),

                        new BinaryExpression(new IdentifierExpression(childAliasName + "._left"),
                            ExpressionOperator.GreaterOperator,
                            new IdentifierExpression(parentAliasName + "._left")),

                        new BinaryExpression(new IdentifierExpression(childAliasName + "._left"), 
                            ExpressionOperator.LessOperator, 
                            new IdentifierExpression(parentAliasName + "._right")),
                    };

            return exps;
        }


    }
}
