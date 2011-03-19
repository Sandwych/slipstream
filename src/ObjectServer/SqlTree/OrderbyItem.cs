using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.SqlTree
{
    public sealed class OrderbyItem : Node, IExpression
    {
        public OrderbyItem(IdentifierExpression idExp, string direction)
        {
            this.Column = idExp;
            this.Direction = direction.Trim().ToUpperInvariant();
        }

        public OrderbyItem(string idExp, string direction)
            : this(new IdentifierExpression(idExp), direction)
        {
        }

        public string Direction { get; private set; }
        public IdentifierExpression Column { get; private set; }

        public override void Traverse(IVisitor visitor)
        {
            visitor.VisitBefore(this);
            visitor.VisitOn(this);
            this.Column.Traverse(visitor);
            visitor.VisitAfter(this);
        }

        public override object Clone()
        {
            throw new NotImplementedException();
        }
    }
}
