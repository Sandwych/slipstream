
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.SqlTree
{
    public sealed class LimitClause : Node, IClause
    {
        public LimitClause(long offset)
        {
            this.Value = new ValueExpression(offset);
        }

        public LimitClause(IExpression valueExp)
        {
            this.Value = valueExp;
        }

        public IExpression Value { get; private set; }

        #region INode 成员

        public override void Traverse(IVisitor visitor)
        {
            visitor.VisitBefore(this);
            visitor.VisitOn(this);
            this.Value.Traverse(visitor);
            visitor.VisitAfter(this);
        }

        #endregion

        #region ICloneable 成员

        public override object Clone()
        {
            var val = (IExpression)this.Value.Clone();
            return new LimitClause(val);
        }

        #endregion
    }
}
