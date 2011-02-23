using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.SqlTree
{
    public class ColumnList : IColumnList
    {
        private string[] _columns;

        public ColumnList(IEnumerable<string> columns)
        {
            this._columns = columns.ToArray();
        }

        public IList<string> Columns
        {
            get { return this._columns; }
        }

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
            return new ColumnList(this.Columns);
        }

        #endregion
    }
}
