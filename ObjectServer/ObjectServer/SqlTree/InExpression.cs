using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.SqlTree
{
    public sealed class InExpression : Node, IExpression
    {
        public InExpression(IExpression lhs, ExpressionGroup values)
        {
            this.Lhs = lhs;
            this.Operator = ExpressionOperator.InOperator;
            this.Values = values;
        }

        public InExpression(string id, IEnumerable<IExpression> values)
            : this(new IdentifierExpression(id), new ExpressionGroup(values))
        {
        }

        public IExpression Lhs { get; set; }
        public ExpressionOperator Operator { get; set; }
        public ExpressionGroup Values { get; set; }

        public override void Traverse(IVisitor visitor)
        {
            visitor.VisitBefore(this);

            this.Lhs.Traverse(visitor);
            this.Operator.Traverse(visitor);
            this.Values.Traverse(visitor);

            visitor.VisitAfter(this);
        }

        public override object Clone()
        {
            throw new NotImplementedException();
        }

    }
}
