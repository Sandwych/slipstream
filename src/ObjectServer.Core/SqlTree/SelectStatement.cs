using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.SqlTree
{
    public class SelectStatement : Node, IStatement
    {
        public DistinctClause DistinctClause { get; set; }
        public IExpression Expression { get; set; }
        public FromClause FromClause { get; set; }
        public WhereClause WhereClause { get; set; }
        public JoinClause[] JoinClauses { get; set; }
        public OrderbyClause OrderByClause { get; set; }
        public OffsetClause OffsetClause { get; set; }
        public LimitClause LimitClause { get; set; }

        public SelectStatement()
        {
        }

        public SelectStatement(IExpression exp, FromClause from)
        {
            this.Expression = exp;
            this.FromClause = from;
        }

        public SelectStatement(IExpression exp, FromClause from, WhereClause where)
        {
            this.Expression = exp;
            this.FromClause = from;
            this.WhereClause = where;
        }

        #region INode 成员

        public override void Traverse(IVisitor visitor)
        {
            if (visitor == null)
            {
                throw new ArgumentNullException("visitor");
            }

            visitor.VisitBefore(this);
            visitor.VisitOn(this);

            if (this.DistinctClause != null)
            {
                this.DistinctClause.Traverse(visitor);
            }

            if (this.Expression != null)
            {
                this.Expression.Traverse(visitor);
            }

            if (this.FromClause != null)
            {
                this.FromClause.Traverse(visitor);
            }

            if (this.JoinClauses != null && this.JoinClauses.Length > 0)
            {
                foreach (var jc in this.JoinClauses)
                {
                    jc.Traverse(visitor);
                }
            }

            if (this.WhereClause != null)
            {
                this.WhereClause.Traverse(visitor);
            }

            if (this.OrderByClause != null)
            {
                this.OrderByClause.Traverse(visitor);
            }

            if (this.OffsetClause != null)
            {
                this.OffsetClause.Traverse(visitor);
            }

            if (this.LimitClause != null)
            {
                this.LimitClause.Traverse(visitor);
            }

            visitor.VisitAfter(this);
        }

        #endregion

        #region ICloneable 成员

        public override object Clone()
        {
            throw new NotImplementedException();
        }

        #endregion


    }
}
