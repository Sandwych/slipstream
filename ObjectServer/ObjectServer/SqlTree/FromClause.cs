
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.SqlTree
{
    public class FromClause : IClause
    {
        private List<Identifier> columns = new List<Identifier>();

        public IList<Identifier> Columns { get { return this.columns; } }

        public FromClause()
        {
        }

        public FromClause(IEnumerable<string> columns)
        {
            foreach (var c in columns)
            {
                this.columns.Add(new Identifier(c));
            }
        }

        #region INode 成员

        public void Traverse(IVisitor visitor)
        {
            visitor.VisitBefore(this);
            visitor.VisitOn(this);
            foreach (var i in this.columns)
            {
                i.Traverse(visitor);
            }
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
