
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.SqlTree
{
    public sealed class DistinctClause : Node, IClause
    {
        public DistinctClause()
        {
            this.Columns = null;
        }

        public DistinctClause(IEnumerable<IdentifierExpression> columns)
        {
            this.Columns = new ExpressionGroup(columns);
        }

        #region INode 成员

        public override void Traverse(IVisitor visitor)
        {
            visitor.VisitBefore(this);
            visitor.VisitOn(this);
            visitor.VisitAfter(this);
        }

        #endregion

        public ExpressionGroup Columns { get; private set; }

        #region ICloneable 成员

        public override object Clone()
        {
            return new DistinctClause();
        }

        #endregion
    }
}
