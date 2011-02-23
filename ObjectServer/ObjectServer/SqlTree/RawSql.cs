using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.SqlTree
{
    public class RawSql : Node
    {
        public RawSql(string sql)
        {
            this.SqlString = sql;
        }

        public string SqlString { get; private set; }

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
            throw new NotImplementedException();
        }

        #endregion
    }
}
