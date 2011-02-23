using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.SqlTree
{
    public class SelectStatement : Node, IStatement
    {
        public FromClause FromClause { get; set; }
        public ColumnList ColumnList { get; set; }

        #region INode 成员

        public override void Traverse(IVisitor visitor)
        {
            visitor.VisitBefore(this);
            visitor.VisitOn(this);

            if (this.ColumnList != null)
            {
                this.ColumnList.Traverse(visitor);
            }

            if (this.FromClause != null)
            {
                this.FromClause.Traverse(visitor);
            }

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
