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

        private LeafDomainCollection leaves;

        private string mainTableAlias;

        IModel model;
        IServiceScope serviceScope;

        public DomainParser(IServiceScope scope, IModel model)
        {
            Debug.Assert(scope != null);
            Debug.Assert(model != null);

            this.leaves = new LeafDomainCollection(model.TableName, model.TableName);
            this.serviceScope = scope;
            this.model = model;
            this.mainTableAlias = model.TableName;
        }

        public Tuple<AliasExpression[], IExpression> Parse(IEnumerable<DomainExpression> domain)
        {

            if (domain == null)
            {
                throw new ArgumentNullException("domain");
            }

            foreach (var o in domain)
            {
                var isBaseField = IsInheritedField(this.serviceScope, this.model, o.Field);
                string aliasedField;

                if (isBaseField)
                {
                    var baseField = ((InheritedField)this.model.Fields[o.Field]).BaseField;
                    var relatedField = this.model.Inheritances
                        .Where(i1 => i1.BaseModel == baseField.Model.Name)
                        .Select(i2 => i2.RelatedField)
                        .Single();
                    var baseTableAlias = this.leaves.PutInnerJoin(baseField.Model.TableName, relatedField);
                    aliasedField = baseTableAlias + '.' + o.Field;
                }
                else
                {
                    aliasedField = this.mainTableAlias + '.' + o.Field;
                }
                this.ParseDomainExpression(aliasedField, o.Operator, o.Value);
            }

            return new Tuple<AliasExpression[], IExpression>(
                this.leaves.GetTableAlias(), this.leaves.GetRestrictionExpression());

        }

        public Tuple<AliasExpression[], IExpression> Parse(IEnumerable<object> domain)
        {
            if (domain == null)
            {
                throw new ArgumentNullException("domain");
            }

            var domains = from o in domain select new DomainExpression(o);
            return this.Parse(domains);
        }

        private static bool IsInheritedField(IServiceScope scope, IModel mainModel, string field)
        {
            Debug.Assert(scope != null);
            Debug.Assert(mainModel != null);
            Debug.Assert(!string.IsNullOrEmpty(field));

            if (mainModel.Inheritances.Count == 0)
            {
                return false;
            }

            if (AbstractTableModel.SystemReadonlyFields.Contains(field))
            {
                return false;
            }

            if (mainModel.Fields[field] is InheritedField)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void ParseDomainExpression(string lhs, string opr, object value)
        {
            Debug.Assert(!string.IsNullOrEmpty(lhs));
            Debug.Assert(!string.IsNullOrEmpty(opr));

            var aliasedField = lhs;

            //如果是引用或者many-to-one类型的字段可以使用 '.' 来访问关联表的字段
            //比如 [["user.organization.code", "=", "main-company"]]
            var fieldParts = lhs.Split('.');
            if (fieldParts.Length > 2)
            {
                var aliasName = PreprocessReferenceField(opr, value, fieldParts);
                aliasedField = aliasName + '.' + fieldParts.Last();
            }

            var exps = new List<IExpression>();

            switch (opr)
            {
                case "=":
                case ">":
                case ">=":
                case "<":
                case "<=":
                case "!=":
                case "like":
                case "!like":
                case "in":
                case "!in":
                    this.leaves.AppendLeaf(aliasedField, opr, value);
                    break;

                case "childof":
                    this.ParseChildOfOperator(aliasedField, value);
                    break;

                case "!childof":
                    throw new NotImplementedException();

                default:
                    throw new NotSupportedException();

            }
        }

        private string PreprocessReferenceField(string opr, object value, string[] fieldParts)
        {
            Debug.Assert(!string.IsNullOrEmpty(opr));
            Debug.Assert(fieldParts != null && fieldParts.Length > 2);

            //检查类型
            var selfFieldName = fieldParts[1];
            var selfField = this.model.Fields[selfFieldName];
            var joinModel = (IModel)this.serviceScope.GetResource(selfField.Relation);
            var joinTableName = joinModel.TableName;
            var aliasName = this.leaves.PutInnerJoin(joinTableName, selfFieldName);
            var aliasIDExpr = aliasName + '.' + AbstractModel.IDFieldName;
            this.leaves.AddJoinRestriction(
                this.model.TableName + '.' + selfFieldName, "=", aliasIDExpr);
            return aliasName;
        }

        private void ParseChildOfOperator(string aliasedField, object value)
        {
            Debug.Assert(!string.IsNullOrEmpty(aliasedField));
            Debug.Assert(aliasedField.Contains('.'));

            //TODO 处理继承字段
            var fieldParts = aliasedField.Split('.');
            var selfField = fieldParts[1];
            var joinTableName = string.Empty;
            if (selfField == AbstractModel.IDFieldName)
            {
                joinTableName = mainTableAlias;
            }
            else
            {
                var fieldInfo = this.model.Fields[selfField];
                var joinModel = (IModel)this.serviceScope.GetResource(fieldInfo.Relation);
                joinTableName = joinModel.TableName;
            }
            //TODO 确认 many2one 类型字段
            var parentAliasName = this.leaves.PutOuterJoin(joinTableName);
            var childAliasName = this.leaves.PutInnerJoin(joinTableName, selfField);


            /* 生成的 SQL 形如：
             * SELECT mainTable._id 
             * FROM mainTable, category _category_parent_0, category AS _category_child_0
             * WHERE _category_child_0._id = mainTable.field AND
             *     _category_parent_0._id = {value} AND
             *     _category_child_0._left > _category_parent_0._left AND
             *     _category_child_0._left < _category_parent_0._right AND ...
             * 
             * */
            //添加约束
            this.leaves.AppendLeaf(parentAliasName + "." + AbstractModel.IDFieldName, "=", value);
            this.leaves.AddJoinRestriction(childAliasName + "._left", ">", parentAliasName + "._left");
            this.leaves.AddJoinRestriction(childAliasName + "._left", "<", parentAliasName + "._right");
        }


    }
}
