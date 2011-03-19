using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.SqlTree
{
    public class UnaryExpression : Node, IExpression
    {

        public UnaryExpression(ExpressionOperator opr, IExpression exp)
        {
            this.Operator = opr;
            this.Expression = exp;
        }


        public ExpressionOperator Operator { get; private set;}
        public IExpression Expression { get; private set; }

        public override void Traverse(IVisitor visitor)
        {
            visitor.VisitBefore(this);
            visitor.VisitOn(this);
            this.Operator.Traverse(visitor);
            this.Expression.Traverse(visitor);
            visitor.VisitAfter(this);

        }

        public override object Clone()
        {
            return new UnaryExpression(
                (ExpressionOperator)this.Operator.Clone(),
                (IExpression)this.Expression.Clone());
        }
    }
}
