using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.SqlTree
{
    public class Identifier : INode
    {
        public Identifier(string id)
        {
            this.Id = id;
        }

        public String Id { get; private set; }


        #region INode 成员

        public void Traverse(IVisitor visitor)
        {
            visitor.VisitBefore(this);
            visitor.VisitOn(this);
            visitor.VisitAfter(this);
        }

        #endregion

        #region ICloneable 成员

        public object Clone()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
