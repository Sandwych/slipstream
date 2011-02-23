using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.SqlTree
{
    public class AliasExpression : Node, IExpression
    {
        private static readonly RawSql asExpression = new RawSql("as");

        public AliasExpression(string lhs, string rhs) :
            this(new IdentifierExpression(lhs), new IdentifierExpression(rhs))
        {
        }

        public AliasExpression(string lhs) :
            this(new IdentifierExpression(lhs), null)
        {
        }

        public AliasExpression(IExpression lhs)
        {
            this.Lhs = lhs;
            this.Rhs = null;
        }

        public AliasExpression(IExpression lhs, IExpression rhs)
        {
            this.Lhs = lhs;
            this.Rhs = rhs;
        }

        public IExpression Lhs { get; private set; }
        public IExpression Rhs { get; private set; }

        public override void Traverse(IVisitor visitor)
        {
            visitor.VisitBefore(this);
            this.Lhs.Traverse(visitor);
            visitor.VisitOn(this);
            if (this.Rhs != null)
            {
                asExpression.Traverse(visitor);
                this.Rhs.Traverse(visitor);
            }
            visitor.VisitAfter(this);

        }

        public override object Clone()
        {
            return new AliasExpression(
               (IExpression)this.Lhs.Clone(),
                (IExpression)this.Rhs.Clone());
        }
    }
}
