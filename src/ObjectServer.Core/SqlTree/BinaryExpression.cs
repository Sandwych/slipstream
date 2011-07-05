using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.SqlTree
{
    public sealed class BinaryExpression : Node, IBinaryExpression
    {

        public BinaryExpression(string id, string opr, object value)
        {
            this.Lhs = new IdentifierExpression(id);
            this.ExpressionOperator = new ExpressionOperator(opr);
            this.Rhs = new ValueExpression(value);
        }

        public BinaryExpression(IExpression lhs, ExpressionOperator opr, IExpression rhs)
        {
            this.Lhs = lhs;
            this.ExpressionOperator = opr;
            this.Rhs = rhs;
        }

        public IExpression Lhs { get; set; }
        public IExpression Rhs { get; set; }
        public ExpressionOperator ExpressionOperator { get; set; }

        public override void Traverse(IVisitor visitor)
        {
            visitor.VisitBefore(this);

            this.Lhs.Traverse(visitor);
            this.ExpressionOperator.Traverse(visitor);
            this.Rhs.Traverse(visitor);

            visitor.VisitAfter(this);
        }

        public override object Clone()
        {
            return new BinaryExpression(
                (IExpression)this.Lhs.Clone(),
                (ExpressionOperator)this.ExpressionOperator.Clone(),
                (IExpression)this.Rhs.Clone());
        }

    }
}
