
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.SqlTree
{
    public sealed class OrderbyClause : Node, IClause
    {
        private IList<OrderbyItem> items;

        public OrderbyClause(string column, string direction)
        {
            this.items = new List<OrderbyItem>(1);
            this.items.Add(
                new OrderbyItem(column, direction));
        }

        public OrderbyClause(IEnumerable<OrderbyItem> items)
        {
            this.items = new List<OrderbyItem>(items);
        }

        public OrderbyClause(IEnumerable<string> ascItems)
        {
            this.items = ascItems.Select(c => new OrderbyItem(c)).ToList();
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
