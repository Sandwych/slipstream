using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ObjectServer.SqlTree;

namespace ObjectServer.Model.Sql
{
    internal class SelectBuilder
    {
        private readonly List<TableJoinInfo> outerJoins = new List<TableJoinInfo>();
        private readonly List<TableJoinInfo> innerJoins = new List<TableJoinInfo>();
        private readonly List<IExpression> whereRestrictions = new List<IExpression>();
        private int joinCount = 0;
        private readonly string mainTable;
        private readonly string mainTableAlias;

        public IList<TableJoinInfo> OuterJoins { get { return this.outerJoins; } }
        public IList<TableJoinInfo> InnerJoins { get { return this.innerJoins; } }
        public IList<IExpression> WhereRestrictions { get { return this.whereRestrictions; } }

        public SelectBuilder(string mainTable, string mainTableAlias)
        {
            this.mainTable = mainTable;
            this.mainTableAlias = mainTableAlias;
        }

        public TableJoinInfo SetOuterJoin(string table, IExpression[] restrictions)
        {
            if (string.IsNullOrEmpty(table))
            {
                throw new ArgumentNullException("table");
            }

            if (restrictions == null)
            {
                throw new ArgumentNullException("restrictions");
            }

            var existedJoin = this.outerJoins.SingleOrDefault(oj => oj.Table == table);
            if (existedJoin != null)
            {
                return existedJoin;
            }
            else
            {
                this.joinCount++;
                string alias = "_t" + this.joinCount.ToString();
                var tj = new TableJoinInfo(table, alias, restrictions);
                this.outerJoins.Add(tj);
                return tj;
            }
        }

        public TableJoinInfo AppendOuterJoin(string table, IExpression[] restrictions)
        {
            if (string.IsNullOrEmpty(table))
            {
                throw new ArgumentNullException("table");
            }

            if (restrictions == null)
            {
                throw new ArgumentNullException("restrictions");
            }

            this.joinCount++;
            string alias = "_t" + this.joinCount.ToString();
            var tj = new TableJoinInfo(table, alias, restrictions);
            this.outerJoins.Add(tj);
            return tj;
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

            this.joinCount++;
            string alias = "_t" + this.joinCount.ToString();
            var joinCond = new BinaryExpression(
                new IdentifierExpression(this.mainTableAlias + "."),
                ExpressionOperator.EqualOperator,
                new IdentifierExpression(alias + "." + field));
            var tj = new TableJoinInfo(table, alias, joinCond);
            this.innerJoins.Add(tj);
            return tj;
        }

        public void SetWhereRestriction(IExpression whereRestriction)
        {
            //TODO 检查重复的约束
            if (whereRestrictions == null)
            {
                throw new ArgumentNullException("whereRestriction");
            }

            this.whereRestrictions.Add(whereRestriction);
        }

    }
}
