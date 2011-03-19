using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.SqlTree
{
    public class Wildcard : INode
    {
        #region INode 成员

        public void Traverse(IVisitor visitor)
        {
            throw new NotImplementedException();
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
