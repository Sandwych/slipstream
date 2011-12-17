using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Diagnostics;

using Malt;
using Autofac;
using NHibernate.SqlCommand;

using ObjectServer.Exceptions;
using ObjectServer.Data;
using ObjectServer.Model;

namespace ObjectServer.Model
{
    internal class SqlQueryBuilder
    {
        private static readonly string TrueSql = " (1=1) ";

        private static readonly string[] TreeParentFields = new string[] { 
                AbstractSqlModel.LeftFieldName, AbstractSqlModel.RightFieldName };

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
        IServiceContext serviceScope;

        public SqlQueryBuilder(IServiceContext scope, IModel rootModel)
        {
            if (scope == null)
            {
                throw new ArgumentNullException("svcCtx");
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

        public SqlQueryBuilder(IServiceContext scope, string rootModelName)
            : this(scope, (IModel)scope.GetResource(rootModelName))
        {

        }

        public void AddOrder(OrderExpression oe)
        {
            if (oe == null)
            {
                throw new ArgumentNullException("oe");
            }

            var field = this.rootModel.Fields[oe.Field];
            if (field.IsColumn)
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
                this.AddOrder(oe);
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
                var tj = new TableJoinInfo(table, alias, fkColumn, AbstractModel.IdFieldName);
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
                var tj = new TableJoinInfo(table, alias, fkColumn, AbstractModel.IdFieldName);
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


        /// <summary>
        /// 每组之间使用 OR 连接，一组中的元素之间使用 AND 连接
        /// </summary>
        /// <param name="constraints"></param>
        public void AddConstraint(IEnumerable<Criterion[]> constraints)
        {
            if (constraints == null)
            {
                throw new ArgumentNullException("constraints");
            }

            if (constraints.Count() > 0)
            {
                this.AddWhereFragment("(");
                var orNeeded = false;
                foreach (var criteria in constraints)
                {
                    if (orNeeded)
                    {
                        this.AddWhereFragment(" or ");
                    }
                    orNeeded = true;

                    this.AddCriteria(criteria);
                }
                this.AddWhereFragment(")");
            }
            else
            {
                this.AddWhereFragment(TrueSql);
            }
        }

        public void AddCriteria(IEnumerable<Criterion> criteria)
        {
            if (criteria == null)
            {
                throw new ArgumentNullException("criteria");
            }

            if (criteria.Count() > 0)
            {
                this.AddWhereFragment("(");
                var andNeeded = false;
                foreach (var c in criteria)
                {
                    if (andNeeded)
                    {
                        this.AddWhereFragment(" and ");
                    }
                    andNeeded = true;
                    this.AddCriterion(c);
                }

                this.AddWhereFragment(")");
            }
            else
            {
                this.AddWhereFragment(TrueSql);
            }
        }

        private void AddCriterion(Criterion criterion)
        {
            if (criterion == null)
            {
                throw new ArgumentNullException("criterion");
            }

            var fieldParts = criterion.Field.Split('.');

            var firstPart = fieldParts.First();
            IField firstPartFieldInfo = null;
            if (!this.rootModel.Fields.TryGetValue(firstPart, out firstPartFieldInfo)
                || firstPartFieldInfo == null)
            {
                var msg = String.Format("Can not found field: [{0}.{1}]", this.rootModel.Name, firstPart);
                throw new ArgumentException("criterion", msg);
            }

            if (!firstPartFieldInfo.Selectable)
            {
                return;
            }

            if (firstPartFieldInfo.IsColumn || firstPartFieldInfo is InheritedField)
            {
                var leafPart = fieldParts.Last();
                var lastModel = this.rootModel;
                var lastTableAlias = MainTableAlias;
                IField field = null;
                foreach (var fieldPart in fieldParts)
                {
                    if (!lastModel.Fields.TryGetValue(fieldPart, out field))
                    {
                        throw new ArgumentOutOfRangeException("criterion");
                    }

                    if (IsInheritedField(lastModel, fieldPart))
                    {
                        lastTableAlias = this.HandleInheritedFieldPart(lastModel, lastTableAlias, fieldPart, field);
                    }

                    //处理连接字段
                    if (field.Type == FieldType.ManyToOne && fieldPart != leafPart)
                    {
                        var relatedModel = (IModel)this.serviceScope.GetResource(field.Relation);
                        lastTableAlias = this.JoinTableByFieldPart(lastTableAlias, field, relatedModel);
                        lastModel = relatedModel;
                    }
                }

                Debug.Assert(field != null);
                Debug.Assert(lastModel != null);

                if (IsInheritedField(lastModel, field.Name))
                {
                    field = ((InheritedField)field).BaseField;
                }
                this.AddLeafColumnCriterion(criterion, lastTableAlias, lastModel, field);
            }
            else
            {
                var criteria = firstPartFieldInfo.CriterionConverter(this.serviceScope, criterion);
                //检查是否包含
                //CriterionConverter 返回的新条件不能包含字段本身，否则将递归
                if (criteria.Count(cr => cr.Field == criterion.Field) > 0)
                {
                    throw new ResourceException("CriterionConverter");
                }

                this.AddCriteria(criteria);
            }
        }

        private string HandleInheritedFieldPart(IModel model, string lastTableAlias, string fieldPart, IField field)
        {
            Debug.Assert(model != null);
            Debug.Assert(!string.IsNullOrEmpty(lastTableAlias));
            Debug.Assert(!string.IsNullOrEmpty(fieldPart));
            Debug.Assert(field != null);
            Debug.Assert(IsInheritedField(model, fieldPart));

            var baseField = ((InheritedField)field).BaseField;
            var relatedField = model.Inheritances
                .Where(i1 => i1.BaseModel == baseField.Model.Name)
                .Select(i2 => i2.RelatedField).Single();
            var baseTableJoin = this.SetInnerJoin(baseField.Model.TableName, relatedField);
            lastTableAlias = baseTableJoin.Alias;
            return lastTableAlias;
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

        private void AddLeafColumnCriterion(
            Criterion cr, string lastTableAlias, IModel model, IField field)
        {
            if (!field.Selectable || !field.IsColumn)
            {
                throw new NotSupportedException();
            }

            switch (cr.Operator)
            {
                case "=":
                case ">":
                case ">=":
                case "<":
                case "<=":
                case "!=":
                    this.AddSimpleLeafCriterion(cr, lastTableAlias, model, field);
                    break;

                case "like":
                case "!like":
                    this.AddLikeAndNotLikeLeafCriterion(cr, lastTableAlias, model, field);
                    break;

                case "in":
                case "!in":
                    this.AddInAndNotInLeafCriterion(cr, lastTableAlias, model, field);
                    break;

                case "childof":
                case "!childof":
                    this.AddChildOfLeafCriterion(cr, lastTableAlias, model, field, cr.Operator);
                    break;

                default:
                    throw new NotSupportedException();
            }

        }

        private void AddSimpleLeafCriterion(
            Criterion cr, string lastTableAlias, IModel model, IField field)
        {
            var column = lastTableAlias + '.' + field.Name;

            if (cr.Operator == "=" && cr.Value.IsNull())
            {
                var whereExp = new SqlString(column, " is null");
                this.AddWhereFragment(whereExp);
            }
            else
            {
                var whereExp = new SqlString(
                    column, " ", cr.Operator, " ", Parameter.WithIndex(this.parameterIndex));
                this.parameterIndex++;
                this.AddWhereFragment(whereExp);
                this.values.Add(cr.Value);

            }

        }

        private void AddLikeAndNotLikeLeafCriterion(
            Criterion cr, string lastTableAlias, IModel model, IField field)
        {
            var opr = cr.Operator == "like" ? "like" : "not like";
            var column = lastTableAlias + '.' + field.Name;
            var whereExp = new SqlString(
                column, " ", opr, " ", Parameter.WithIndex(this.parameterIndex));
            this.parameterIndex++;
            this.AddWhereFragment(whereExp);
            this.values.Add(cr.Value);
        }

        private void AddInAndNotInLeafCriterion(
            Criterion cr, string lastTableAlias, IModel model, IField field)
        {
            if (cr.Value == null)
            {
                throw new ArgumentException(
                    "The sequence of 'in/!in' operator can not be null", "criterion");
            }

            var opr = cr.Operator == "in" ? "in" : "not in";
            var objs = (System.Collections.IEnumerable)cr.Value;
            var inValues = objs.Cast<object>().ToArray();

            if (!inValues.Any())
            {
                throw new ArgumentException(
                    "The sequence of 'in/!in' operator can not be empty", "criterion");
            }

            var expBuilder = new SqlStringBuilder();
            expBuilder.Add(lastTableAlias + '.' + field.Name);
            expBuilder.Add(" ");
            expBuilder.Add(opr);
            expBuilder.Add("(");

            for (int i = 0; i < inValues.Length; i++)
            {
                if (inValues.Length > 1 && i != 0)
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

        private void AddChildOfLeafCriterion(
            Criterion cr, string lastTableAlias, IModel model, IField field, string opr)
        {
            if (!(cr.Value is long || cr.Value is int)) //防止 SQL 注入
            {
                throw new ArgumentOutOfRangeException("cr");
            }

            /* 生成的 SQL 形如：
             * SELECT mainTable._id 
             * FROM mainTable, category _category_parent_0, category AS _category_child_0
             * WHERE _category_child_0._id = mainTable.field AND
             *     _category_parent_0._id = {value} AND
             *     _category_child_0._left > _category_parent_0._left AND
             *     _category_child_0._left < _category_parent_0._right AND ...
                    * */
            IModel joinModel = null;
            if (field.Name == AbstractModel.IdFieldName)
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
            var parent = this.ReadSingleTreeRecord(cr, joinModel);

            var parentLeft = parent[AbstractSqlModel.LeftFieldName].ToString();
            var parentRight = parent[AbstractSqlModel.RightFieldName].ToString();

            //添加 Parent 连接
            var leftExp = new SqlString(
                childTableAlias.Alias + "." + AbstractSqlModel.LeftFieldName,
                ">",
                parentLeft);

            var rightExp = new SqlString(
                childTableAlias.Alias + "." + AbstractSqlModel.LeftFieldName,
                "<",
                parentRight);

            if (opr == "childof")
            {
                this.AddWhereFragment(new SqlString(leftExp, " and ", rightExp));
            }
            else
            {
                this.AddWhereFragment(new SqlString(" not (", leftExp, " and ", rightExp, ")"));
            }
        }

        private Dictionary<string, object> ReadSingleTreeRecord(Criterion cr, IModel joinModel)
        {
            var parentCriteria = new object[] {
                new object[] { AbstractModel.IdFieldName, "=", cr.Value }
            };
            var parentIDs = new long[] { (long)cr.Value };
            var parents = joinModel.ReadInternal(this.serviceScope, parentIDs, TreeParentFields);
            if (parents.Length != 1)
            {
                var msg = string.Format(
                    "Cannot found the record with ID=[{0}]", cr.Value.ToString());
                throw new RecordNotFoundException(msg, joinModel.Name);
            }
            var parent = parents.First();
            return parent;
        }

        private static bool IsInheritedField(IModel mainModel, string field)
        {
            Debug.Assert(mainModel != null);
            Debug.Assert(!string.IsNullOrEmpty(field));

            if (mainModel.Inheritances.Count == 0)
            {
                return false;
            }

            if (AbstractSqlModel.SystemReadonlyFields.Contains(field))
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

        public SqlString ToSqlString(bool isCount = false)
        {
            var dataProvider = SlipstreamEnvironment.RootContainer.Resolve<IDataProvider>();
            var qs = new QuerySelect(dataProvider.Dialect);

            SqlString columnsFragment = null;
            if (isCount)
            {
                columnsFragment = new SqlString("count(", MainTableAlias + '.' + AbstractModel.IdFieldName, ")");
            }
            else
            {
                columnsFragment = new SqlString(MainTableAlias + '.' + AbstractModel.IdFieldName);
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
    }
}
