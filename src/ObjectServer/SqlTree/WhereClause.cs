
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.SqlTree
{
    public class WhereClause : IClause
    {
        public WhereClause(IExpression exp)
        {
            this.Expression = exp;
        }

        public IExpression Expression { get; private set; }

        #region INode 成员

        public void Traverse(IVisitor visitor)
        {            
            visitor.VisitBefore(this);
            visitor.VisitOn(this);
            this.Expression.Traverse(visitor);
            visitor.VisitAfter(this);
        }

        #endregion

        #region ICloneable 成员

        public object Clone()
        {
            throw new System.NotImplementedException();
        }

        #endregion
    }
}
