using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using ObjectServer.SqlTree;
using ObjectServer.Model.Sql;

namespace ObjectServer.Model
{
    internal sealed class ConstraintBuilder
    {
        public static readonly string[] Operators = new string[]
        {
            "=", "!=", ">", ">=", "<", "<=", "in", "!in", 
            "like", "!like", "childof", "!childof"
        };

        public const string MainTableAlias = "_t0";
        private readonly SelectBuilder selectBuilder;

        IModel mainModel;
        IServiceScope serviceScope;

        public ConstraintBuilder(IServiceScope scope, IModel mainModel, SelectBuilder selectBuilder)
        {
            if (scope == null)
            {
                throw new ArgumentNullException("scope");
            }

            if (mainModel == null)
            {
                throw new ArgumentNullException("mainModel");
            }

            if (selectBuilder == null)
            {
                throw new ArgumentNullException("selectBuilder");
            }

            this.serviceScope = scope;
            this.mainModel = mainModel;
            this.selectBuilder = selectBuilder;
        }

        public ConstraintBuilder(IServiceScope scope, string model, SelectBuilder selectBuilder)

        {
           
        }

        public void Add(ConstraintExpression constraint)
        {
            if (constraint == null)
            {
                throw new ArgumentNullException("constraint");
            }

            var fieldParts = constraint.Field.Split('.');

            var lastModel = this.mainModel;
            var lastTableAlias = string.Empty;
            foreach (var fieldPart in fieldParts)
            {
                var field = lastModel.Fields[fieldPart];
                var fieldName = field.Name;

                //处理继承字段
                if (IsInheritedField(lastModel, fieldPart))
                {
                    var baseField = ((InheritedField)field).BaseField;
                    var relatedField = lastModel.Inheritances
                        .Where(i1 => i1.BaseModel == baseField.Model.Name)
                        .Select(i2 => i2.RelatedField).Single();
                    var baseTableJoin = this.selectBuilder.SetInnerJoin(baseField.Model.TableName, relatedField);
                    fieldName = baseTableJoin.Alias + '.' + field.Name;
                }

                //处理连接字段
                //TODO 处理reference/property 类型字段
                if (field.Type == FieldType.Reference || field.Type == FieldType.ManyToOne)
                {
                    IModel relatedModel = (IModel)this.serviceScope.GetResource(field.Relation);
                    if (field.IsRequired)
                    {
                        lastTableAlias = selectBuilder.SetInnerJoin(relatedModel.TableName, fieldName).Alias;
                    }
                    else
                    {
                        lastTableAlias = selectBuilder.AppendOuterJoin(relatedModel.TableName, fieldName).Alias;
                    }

                    lastModel = relatedModel;
                }
                else //否则则为叶子节点 
                {
                    //TODO 处理 childof 运算符
                    var whereExp = new BinaryExpression(
                        lastTableAlias + '.' + fieldName, constraint.Operator, constraint.Value);
                    selectBuilder.SetWhereRestriction(whereExp);
                }
            }

        }

        private bool IsInheritedField(IModel mainModel, string field)
        {
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

    }
}
