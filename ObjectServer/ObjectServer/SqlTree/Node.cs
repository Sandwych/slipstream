using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.SqlTree
{
    public abstract class Node : INode
    {

        public override string ToString()
        {
            var visitor = new StringifierVisitor();
            this.Traverse(visitor);
            return visitor.ToString();
        }

        #region INode 成员

        public abstract void Traverse(IVisitor visitor);

        #endregion

        #region ICloneable 成员

        public abstract object Clone();

        #endregion
    }
}
