using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Diagnostics;

using NHibernate.SqlCommand;

using ObjectServer.Model;

namespace ObjectServer.Sql
{
    internal class QueryTranslator
    {
        private readonly List<TableJoinInfo> outerJoins = new List<TableJoinInfo>();
        private readonly List<TableJoinInfo> innerJoins = new List<TableJoinInfo>();
        private readonly List<SqlString> whereRestrictions = new List<SqlString>();
        private readonly List<object> values = new List<object>();
        private int joinCount = 0;

        public IList<TableJoinInfo> OuterJoins { get { return this.outerJoins; } }
        public IList<TableJoinInfo> InnerJoins { get { return this.innerJoins; } }
        public IList<SqlString> WhereRestrictions { get { return this.whereRestrictions; } }
        public IList<object> Values { get { return this.values; } }

        public static readonly string[] Operators = new string[]
        {
            "=", "!=", ">", ">=", "<", "<=", "in", "!in", 
            "like", "!like", "childof", "!childof"
        };

        public const string MainTableAlias = "_t0";

        IModel rootModel;
        IServiceScope serviceScope;

        public QueryTranslator(IServiceScope scope, IModel rootModel)
        {
            if (scope == null)
            {
                throw new ArgumentNullException("scope");
            }

            if (rootModel == null)
            {
                throw new ArgumentNullException("mainModel");
            }

            this.serviceScope = scope;
            this.rootModel = rootModel;
        }

        public QueryTranslator(IServiceScope scope, string rootModelName)
            : this(scope, (IModel)scope.GetResource(rootModelName))
        {

        }

        public TableJoinInfo SetOuterJoin(string table, string field)
        {
            if (string.IsNullOrEmpty(table))
            {
                throw new ArgumentNullException("table");
            }

            if (string.IsNullOrEmpty(field))
            {
                throw new ArgumentNullException("field");
            }

            var fkColumn = MainTableAlias + '.' + field;
            var existed = this.outerJoins.SingleOrDefault(j => j.FkColumn == fkColumn);

            if (existed == null)
            {

                this.joinCount++;
                string alias = "_t" + this.joinCount.ToString();
                var tj = new TableJoinInfo(table, alias, fkColumn, AbstractModel.IDFieldName);
                this.outerJoins.Add(tj);
                return tj;
            }
            else
            {
                return existed;
            }
        }

        public TableJoinInfo SetInnerJoin(string table, string field)
        {
            if (string.IsNullOrEmpty(table))
            {
                throw new ArgumentNullException("table");
            }

            if (string.IsNullOrEmpty(field))
            {
                throw new ArgumentNullException("field");
            }

            var fkColumn = MainTableAlias + '.' + field;
            var existed = this.innerJoins.SingleOrDefault(j => j.FkColumn == fkColumn);

            if (existed == null)
            {

                this.joinCount++;
                string alias = "_t" + this.joinCount.ToString();
                var tj = new TableJoinInfo(table, alias, fkColumn, AbstractModel.IDFieldName);
                this.innerJoins.Add(tj);
                return tj;
            }
            else
            {
                return existed;
            }
        }

        public void SetWhereRestriction(SqlString whereRestriction)
        {
            //TODO 检查重复的约束
            if (whereRestrictions == null)
            {
                throw new ArgumentNullException("whereRestriction");
            }

            this.whereRestrictions.Add(whereRestriction);
        }

        public SqlString ToSqlString()
        {
            var qs = new QuerySelect(Data.DataProvider.Dialect);

            var columnsFragment = new SqlString(MainTableAlias + '.' + AbstractModel.IDFieldName);
            qs.AddSelectFragmentString(columnsFragment);
            qs.Distinct = true;

            var fromClause = new SqlString(" ", this.rootModel.TableName, " ", MainTableAlias);
            qs.JoinFragment.AddJoins(fromClause, SqlString.Empty);

            foreach (var innerJoin in this.InnerJoins)
            {
                qs.JoinFragment.AddJoin(
                    innerJoin.Table, innerJoin.Alias,
                    new string[] { innerJoin.FkColumn },
                    new string[] { innerJoin.PkColumn }, JoinType.InnerJoin);
            }

            foreach (var outerJoin in this.OuterJoins)
            {
                qs.JoinFragment.AddJoin(
                    outerJoin.Table, outerJoin.Alias,
                    new string[] { outerJoin.FkColumn },
                    new string[] { outerJoin.PkColumn }, JoinType.LeftOuterJoin);
            }

            var whereTokens = new SqlString[] { this.whereRestrictions.JoinByAnd() };
            qs.SetWhereTokens((ICollection)whereTokens);

            return qs.ToQuerySqlString();
        }

        public void Add(ConstraintExpression constraint)
        {
            if (constraint == null)
            {
                throw new ArgumentNullException("constraint");
            }

            var fieldParts = constraint.Field.Split('.');

            var lastModel = this.rootModel;
            var lastTableAlias = MainTableAlias;
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
                    var baseTableJoin = this.SetInnerJoin(baseField.Model.TableName, relatedField);
                    fieldName = baseTableJoin.Alias + '.' + field.Name;
                }

                //处理连接字段
                //TODO 处理reference/property 类型字段
                if (field.Type == FieldType.Reference || field.Type == FieldType.ManyToOne)
                {
                    IModel relatedModel = (IModel)this.serviceScope.GetResource(field.Relation);
                    if (field.IsRequired)
                    {
                        lastTableAlias = this.SetInnerJoin(relatedModel.TableName, fieldName).Alias;
                    }
                    else
                    {
                        lastTableAlias = this.SetOuterJoin(relatedModel.TableName, fieldName).Alias;
                    }

                    lastModel = relatedModel;
                }
                else //否则则为叶子节点 
                {
                    //TODO 处理 childof 运算符
                    var column = lastTableAlias + '.' + fieldName;
                    var whereExp = new SqlString(column, constraint.Operator, Parameter.Placeholder);
                    this.whereRestrictions.Add(whereExp);
                    this.values.Add(constraint.Value);
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
