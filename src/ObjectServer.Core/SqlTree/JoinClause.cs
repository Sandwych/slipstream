using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.SqlTree
{
    public class JoinClause : Node, IClause
    {
        public JoinClause(string joinType, AliasExpression joinSource, IExpression joinCond)
        {
            this.JoinType = joinType;
            this.JoinCondition = joinCond;
            this.JoinSource = joinSource;
        }

        public IExpression JoinCondition { get; private set; }
        public AliasExpression JoinSource { get; private set; }
        public string JoinType { get; private set; }

        public override void Traverse(IVisitor visitor)
        {
            visitor.VisitBefore(this);

            this.JoinSource.Traverse(visitor);

            visitor.VisitOn(this);

            this.JoinCondition.Traverse(visitor);

            visitor.VisitAfter(this);
        }

        public override object Clone()
        {
            throw new NotImplementedException();
        }
    }
}
