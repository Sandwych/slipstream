using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.SqlTree
{
    public class IdentifierExpression : Node, IExpression
    {
        public IdentifierExpression(string id)
        {
            this.Id = id;
        }

        public String Id { get; private set; }


        #region INode 成员

        public override void Traverse(IVisitor visitor)
        {
            visitor.VisitBefore(this);
            visitor.VisitOn(this);
            visitor.VisitAfter(this);
        }

        #endregion

        #region ICloneable 成员

        public override object Clone()
        {
            return new IdentifierExpression(this.Id);
        }

        #endregion
    }
}
