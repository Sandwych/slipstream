using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Diagnostics;

using NHibernate.SqlCommand;

using ObjectServer.Exceptions;
using ObjectServer.Data;
using ObjectServer.Model;

namespace ObjectServer.Sql
{
    internal class ConstraintTranslator
    {
        private static readonly string s_trueSql =
            ' ' + DataProvider.Dialect.ToBooleanValueString(true) + ' ';

        private static readonly string[] s_treeParentFields = new string[] { 
                AbstractTableModel.LeftFieldName, AbstractTableModel.RightFieldName };

        private readonly List<TableJoinInfo> outerJoins = new List<TableJoinInfo>();
        private readonly List<TableJoinInfo> innerJoins = new List<TableJoinInfo>();
        private readonly List<SqlString> fromJoins = new List<SqlString>();
        private readonly SqlStringBuilder whereRestrictions = new SqlStringBuilder();
        private readonly List<object> values = new List<object>();
        private readonly List<OrderExpression> orders = new List<OrderExpression>();
        private int joinCount = 0;
        private int parameterIndex = 0;

        public object[] Values { get { return this.values.ToArray(); } }

        public const string MainTableAlias = "_t0";

        IModel rootModel;
        ITransactionContext serviceScope;

        public ConstraintTranslator(ITransactionContext scope, IModel rootModel)
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

            var rootFormClause = new SqlString(this.rootModel.TableName, " ", MainTableAlias);
            this.fromJoins.Add(rootFormClause);
        }

        public ConstraintTranslator(ITransactionContext scope, string rootModelName)
            : this(scope, (IModel)scope.GetResource(rootModelName))
        {

        }

        public void SetOrder(OrderExpression oe)
        {
            if (oe == null)
            {
                throw new ArgumentNullException("oe");
            }

            var field = this.rootModel.Fields[oe.Field];
            if (field.IsColumn())
            {
                this.orders.Add(oe);
            }
        }

        public void SetOrders(IEnumerable<OrderExpression> oes)
        {
            if (oes == null)
            {
                throw new ArgumentNullException("oes");
            }

            foreach (var oe in oes)
            {
                this.SetOrder(oe);
            }
        }

        private string GenerateNextAlias()
        {
            this.joinCount++;
            return "_t" + this.joinCount.ToString();
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
                string alias = this.GenerateNextAlias();
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
                string alias = this.GenerateNextAlias();
                var tj = new TableJoinInfo(table, alias, fkColumn, AbstractModel.IDFieldName);
                this.innerJoins.Add(tj);
                return tj;
            }
            else
            {
                return existed;
            }
        }

        public void AddWhereFragment(SqlString whereFragment)
        {
            if (whereFragment == null)
            {
                throw new ArgumentNullException("whereFragment");
            }

            this.whereRestrictions.Add(" ");
            this.whereRestrictions.Add(whereFragment);
            this.whereRestrictions.Add(" ");
        }

        public void AddWhereFragment(string whereFragment)
        {
            if (string.IsNullOrEmpty(whereFragment))
            {
                throw new ArgumentNullException("whereFragment");
            }

            this.whereRestrictions.Add(whereFragment);
        }

        public SqlString ToSqlString(bool isCount = false)
        {
            var qs = new QuerySelect(Data.DataProvider.Dialect);

            SqlString columnsFragment = null;
            if (isCount)
            {
                columnsFragment = new SqlString("count(", MainTableAlias + '.' + AbstractModel.IDFieldName, ")");
            }
            else
            {
                columnsFragment = new SqlString(MainTableAlias + '.' + AbstractModel.IDFieldName);
            }
            qs.AddSelectFragmentString(columnsFragment);
            qs.Distinct = false;

            var fromClauseBuilder = new SqlStringBuilder();
            for (int i = 0; i < this.fromJoins.Count; i++)
            {
                if (this.fromJoins.Count > 1 && i > 0)
                {
                    fromClauseBuilder.Add(", ");
                }
                else
                {
                    fromClauseBuilder.Add(" ");
                }
                fromClauseBuilder.Add(this.fromJoins[i]);
                fromClauseBuilder.Add(" ");
            }
            var fromClause = fromClauseBuilder.ToSqlString();
            qs.JoinFragment.AddJoins(fromClause, SqlString.Empty);

            foreach (var innerJoin in this.innerJoins)
            {
                qs.JoinFragment.AddJoin(
                    innerJoin.Table, innerJoin.Alias,
                    new string[] { innerJoin.FkColumn },
                    new string[] { innerJoin.PkColumn }, JoinType.InnerJoin);
            }

            foreach (var outerJoin in this.outerJoins)
            {
                qs.JoinFragment.AddJoin(
                    outerJoin.Table, outerJoin.Alias,
                    new string[] { outerJoin.FkColumn },
                    new string[] { outerJoin.PkColumn }, JoinType.LeftOuterJoin);
            }

            if (this.whereRestrictions.Count > 0)
            {
                var whereTokens = new SqlString[] { this.whereRestrictions.ToSqlString() };
                qs.SetWhereTokens((ICollection)whereTokens);
            }

            if (!isCount)
            {
                foreach (var o in this.orders)
                {
                    var orderBySql = ' ' + MainTableAlias + '.' + o.Field + ' ' + o.Order.ToSql();
                    qs.AddOrderBy(orderBySql);
                }
            }

            return qs.ToQuerySqlString();
        }

        /// <summary>
        /// 每组之间使用 OR 连接，一组中的元素之间使用 AND 连接
        /// </summary>
        /// <param name="ruleConstraints"></param>
        public void AddGroupedConstraints(IEnumerable<ConstraintExpression[]> ruleConstraints)
        {
            if (ruleConstraints.Count() > 0)
            {
                this.AddWhereFragment("(");
                var orNeeded = false;
                foreach (var constraints in ruleConstraints)
                {
                    if (orNeeded)
                    {
                        this.AddWhereFragment(" or ");
                    }
                    orNeeded = true;

                    this.AddConstraints(constraints);
                }
                this.AddWhereFragment(")");
            }
            else
            {
                this.AddWhereFragment(s_trueSql);
            }
        }

        public void AddConstraints(IEnumerable<ConstraintExpression> constraints)
        {
            if (constraints.Count() > 0)
            {
                this.AddWhereFragment("(");
                var andNeeded = false;
                foreach (var c in constraints)
                {
                    if (andNeeded)
                    {
                        this.AddWhereFragment(" and ");
                    }
                    andNeeded = true;
                    this.AddConstraint(c);
                }

                this.AddWhereFragment(")");
            }
            else
            {
                this.AddWhereFragment(s_trueSql);
            }
        }

        private void AddConstraint(ConstraintExpression constraint)
        {
            if (constraint == null)
            {
                throw new ArgumentNullException("constraint");
            }

            var fieldParts = constraint.Field.Split('.');

            var leafPart = fieldParts.Last();
            var lastModel = this.rootModel;
            var lastTableAlias = MainTableAlias;
            foreach (var fieldPart in fieldParts)
            {
                IField field = null;
                if (!lastModel.Fields.TryGetValue(fieldPart, out field))
                {
                    throw new ArgumentOutOfRangeException("constraint");
                }

                lastTableAlias = this.ProcessInheritedFieldPart(lastModel, lastTableAlias, fieldPart, field);

                //处理连接字段
                if (field.Type == FieldType.ManyToOne && fieldPart != leafPart)
                {
                    IModel relatedModel = (IModel)this.serviceScope.GetResource(field.Relation);
                    lastTableAlias = this.JoinTableByFieldPart(lastTableAlias, field, relatedModel);
                    lastModel = relatedModel;
                }
                else //否则则为叶子节点 
                {
                    this.ProcessLeafNode(constraint, lastTableAlias, lastModel, field);
                }
            }
        }

        private string JoinTableByFieldPart(string lastTableAlias, IField field, IModel relatedModel)
        {
            if (field.IsRequired)
            {
                lastTableAlias = this.SetInnerJoin(relatedModel.TableName, field.Name).Alias;
            }
            else
            {
                lastTableAlias = this.SetOuterJoin(relatedModel.TableName, field.Name).Alias;
            }
            return lastTableAlias;
        }

        private string ProcessInheritedFieldPart(IModel lastModel, string lastTableAlias, string fieldPart, IField field)
        {
            //处理继承字段
            if (IsInheritedField(lastModel, fieldPart))
            {
                var baseField = ((InheritedField)field).BaseField;
                var relatedField = lastModel.Inheritances
                    .Where(i1 => i1.BaseModel == baseField.Model.Name)
                    .Select(i2 => i2.RelatedField).Single();
                var baseTableJoin = this.SetInnerJoin(baseField.Model.TableName, relatedField);
                lastTableAlias = baseTableJoin.Alias;
            }
            return lastTableAlias;
        }

        private void ProcessLeafNode(
            ConstraintExpression constraint, string lastTableAlias, IModel model, IField field)
        {
            if (field.IsFunctional)
            {
                throw new NotSupportedException();
            }

            switch (constraint.Operator)
            {
                case "=":
                case ">":
                case ">=":
                case "<":
                case "<=":
                case "!=":
                    this.ProcessSimpleLeafNode(constraint, lastTableAlias, model, field);
                    break;

                case "like":
                case "!like":
                    this.ProcessLikeAndNotLikeNode(constraint, lastTableAlias, model, field);
                    break;

                case "in":
                case "!in":
                    this.ProcessInAndNotInNode(constraint, lastTableAlias, model, field);
                    break;

                case "childof":
                    this.ProcessChildOfNode(constraint, lastTableAlias, model, field);
                    break;

                case "!childof":
                    throw new NotImplementedException();


                default:
                    throw new NotSupportedException();
            }

        }

        private void ProcessSimpleLeafNode(
            ConstraintExpression constraint, string lastTableAlias, IModel model, IField field)
        {
            var column = lastTableAlias + '.' + field.Name;

            if (constraint.Operator == "=" && constraint.Value.IsNull())
            {
                var whereExp = new SqlString(column, " is null");
                this.AddWhereFragment(whereExp);
            }
            else
            {
                var whereExp = new SqlString(
                    column, " ", constraint.Operator, " ", Parameter.WithIndex(this.parameterIndex));
                this.parameterIndex++;
                this.AddWhereFragment(whereExp);
                this.values.Add(constraint.Value);

            }

        }

        private void ProcessLikeAndNotLikeNode(
            ConstraintExpression constraint, string lastTableAlias, IModel model, IField field)
        {
            var opr = constraint.Operator == "like" ? "like" : "not like";
            var column = lastTableAlias + '.' + field.Name;
            var whereExp = new SqlString(
                column, " ", opr, " ", Parameter.WithIndex(this.parameterIndex));
            this.parameterIndex++;
            this.AddWhereFragment(whereExp);
            this.values.Add(constraint.Value);
        }

        private void ProcessInAndNotInNode(
            ConstraintExpression constraint, string lastTableAlias, IModel model, IField field)
        {
            var opr = constraint.Operator == "in" ? "in" : "not in";
            var inValues = (ICollection<object>)constraint.Value;

            var expBuilder = new SqlStringBuilder();
            expBuilder.Add(lastTableAlias + '.' + field.Name);
            expBuilder.Add(" ");
            expBuilder.Add(opr);
            expBuilder.Add("(");

            for (int i = 0; i < inValues.Count; i++)
            {
                if (inValues.Count > 1 && i != 0)
                {
                    expBuilder.Add(",");
                }
                this.parameterIndex++;
                expBuilder.AddParameter();
            }
            expBuilder.Add(")");

            this.AddWhereFragment(expBuilder.ToSqlString());
            this.values.AddRange(inValues);
        }

        private void ProcessChildOfNode(
            ConstraintExpression constraint, string lastTableAlias, IModel model, IField field)
        {
            /* 生成的 SQL 形如：
             * SELECT mainTable._id 
             * FROM mainTable, category _category_parent_0, category AS _category_child_0
             * WHERE _category_child_0._id = mainTable.field AND
             *     _category_parent_0._id = {value} AND
             *     _category_child_0._left > _category_parent_0._left AND
             *     _category_child_0._left < _category_parent_0._right AND ...
                    * */
            IModel joinModel = null;
            if (field.Name == AbstractModel.IDFieldName)
            {
                joinModel = model;
            }
            else
            {
                if (field.Type != FieldType.ManyToOne)
                {
                    throw new NotSupportedException();
                }

                joinModel = (IModel)this.serviceScope.GetResource(field.Relation);
            }

            //添加 child 连接
            var childTableAlias = this.SetInnerJoin(joinModel.TableName, field.Name);

            //直接做一次查询
            var parent = this.ReadSingleTreeModel(constraint, joinModel);

            var parentLeft = parent[AbstractTableModel.LeftFieldName].ToString();
            var parentRight = parent[AbstractTableModel.RightFieldName].ToString();

            //添加 Parent 连接
            var leftExp = new SqlString(
                childTableAlias.Alias + "." + AbstractTableModel.LeftFieldName,
                ">",
                parentLeft);

            var rightExp = new SqlString(
                childTableAlias.Alias + "." + AbstractTableModel.LeftFieldName,
                "<",
                parentRight);

            this.AddWhereFragment(new SqlString(leftExp, " and ", rightExp));
        }

        private Dictionary<string, object> ReadSingleTreeModel(ConstraintExpression constraint, IModel joinModel)
        {
            var parentConstraints = new object[]{
                new object[] { "_id", "=", constraint.Value }
            };
            var parentIDs = new long[] { (long)constraint.Value };
            var parents = joinModel.ReadInternal(this.serviceScope, parentIDs, s_treeParentFields);
            if (parents.Length != 1)
            {
                var msg = string.Format(
                    "Cannot found the record with ID=[{0}]", constraint.Value.ToString());
                throw new RecordNotFoundException(msg, joinModel.Name);
            }
            var parent = parents[0];
            return parent;
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
