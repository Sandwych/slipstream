
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.SqlTree
{
    public sealed class OrderbyClause : Node, IClause
    {
        private IList<OrderbyItem> items = new List<OrderbyItem>();

        public OrderbyClause(string column, string direction)
        {
            this.items.Add(
                new OrderbyItem(column, direction));
        }

        public IList<OrderbyItem> Items
        {
            get
            {
                return this.items;
            }
        }

        #region INode 成员

        public override void Traverse(IVisitor visitor)
        {
            visitor.VisitBefore(this);
            visitor.VisitOn(this);
            foreach (var idExp in this.Items)
            {
                idExp.Traverse(visitor);
            }
            visitor.VisitAfter(this);
        }

        #endregion

        #region ICloneable 成员

        public override object Clone()
        {
            throw new System.NotImplementedException();
        }

        #endregion
    }
}
