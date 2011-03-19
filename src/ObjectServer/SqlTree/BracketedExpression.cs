using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.SqlTree
{
    public class BracketedExpression : Node, IExpression
    {

        public BracketedExpression(IExpression exp)
        {
            this.Expression = exp;
        }

        public IExpression Expression { get; private set; }

        public override void Traverse(IVisitor visitor)
        {
            visitor.VisitBefore(this);
            visitor.VisitOn(this);
            this.Expression.Traverse(visitor);
            visitor.VisitAfter(this);

        }

        public override object Clone()
        {
            return new BracketedExpression((IExpression)this.Expression.Clone());
        }
    }
}
