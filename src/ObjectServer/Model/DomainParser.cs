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

        private static readonly List<DomainExpression> EmptyDomain = new List<DomainExpression>();
        private LeafDomainCollection leaves;

        private string mainTableAlias;

        IModel model;
        IServiceScope serviceScope;
        List<DomainExpression> internalDomain = new List<DomainExpression>();

        public DomainParser(IServiceScope scope, IModel model, IEnumerable<object> domain)
        {
            Debug.Assert(scope != null);
            Debug.Assert(model != null);

            //TODO 过滤掉不能处理的字段，比如函数字段等
            if (domain == null || domain.Count() <= 0)
            {
                this.internalDomain = EmptyDomain;
            }


            this.leaves = new LeafDomainCollection(model.TableName, model.TableName);
            this.serviceScope = scope;
            this.model = model;
            this.mainTableAlias = model.TableName;
        }

        public Tuple<AliasExpression[], IExpression> Parse(IEnumerable<object> domain)
        {
            foreach (object[] o in domain)
            {

                var de = (object[])o;
                var field = (string)o[0];
                var opr = (string)o[1];

                var isBaseField = IsInheritedField(this.serviceScope, this.model, field);
                string aliasedField;

                if (isBaseField)
                {
                    var baseField = ((InheritedField)this.model.Fields[field]).BaseField;
                    var relatedField = this.model.Inheritances
                        .Where(i1 => i1.BaseModel == baseField.Model.Name)
                        .Select(i2 => i2.RelatedField)
                        .Single();
                    var baseTableAlias = this.leaves.PutInnerJoin(baseField.Model.TableName, relatedField);
                    aliasedField = baseTableAlias + '.' + field;
                }
                else
                {
                    aliasedField = this.mainTableAlias + '.' + field;
                }
                this.ParseDomainExpression(aliasedField, opr, de[2]);
            }

            return new Tuple<AliasExpression[], IExpression>(
                this.leaves.GetTableAlias(), this.leaves.GetRestrictionExpression());
        }

        private static bool IsInheritedField(IServiceScope scope, IModel mainModel, string field)
        {
            Debug.Assert(mainModel != null);

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

        public bool ContainsField(string field)
        {
            return this.internalDomain.Exists(exp => exp.Field == field);
        }

        private void ParseDomainExpression(string lhs, string opr, object value)
        {
            var aliasedField = lhs;

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

        private void ParseChildOfOperator(string aliasedField, object value)
        {
            Debug.Assert(!string.IsNullOrEmpty(aliasedField));
            Debug.Assert(aliasedField.Contains('.'));

            //TODO 处理继承字段
            var fieldParts = aliasedField.Split('.');
            var selfField = fieldParts[1];
            var joinTableName = string.Empty;
            if (selfField == "id")
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
             * SELECT mainTable.id 
             * FROM mainTable, category _category_parent_0, category AS _category_child_0
             * WHERE _category_child_0.id = mainTable.field AND
             *     _category_parent_0.id = {value} AND
             *     _category_child_0._left > _category_parent_0._left AND
             *     _category_child_0._left < _category_parent_0._right AND ...
             * 
             * */
            //添加约束
            this.leaves.AppendLeaf(parentAliasName + ".id", "=", value);
            this.leaves.AddJoinRestriction(childAliasName + "._left", ">", parentAliasName + "._left");
            this.leaves.AddJoinRestriction(childAliasName + "._left", "<", parentAliasName + "._right");
        }


    }
}
