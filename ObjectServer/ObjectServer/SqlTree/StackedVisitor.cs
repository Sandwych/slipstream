using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace ObjectServer.SqlTree
{
    public abstract class StackedVisitor : IVisitor
    {
        private Stack<INode> _path = new Stack<INode>();

        public Stack<INode> Path { get { return this._path; } }


        public void Push(INode node)
        {
            this.Path.Push(node);
        }

        public void Pop()
        {
            this.Path.Pop();
        }

        public INode Parent { get { return this.Path.Peek(); } }

        #region IVisitor 成员

        public void VisitBefore(Identifier node) { this.Push(node); }
        public virtual void VisitOn(Identifier node) { }
        public void VisitAfter(Identifier node)
        {
            Debug.Assert(object.ReferenceEquals(this.Parent, node));
            this.Pop();
        }

        public void VisitBefore(SelectStatement node) { this.Push(node); }
        public virtual void VisitOn(SelectStatement node) { }
        public void VisitAfter(SelectStatement node)
        {
            Debug.Assert(object.ReferenceEquals(this.Parent, node));
            this.Pop();
        }

        public void VisitBefore(ColumnList node) { this.Push(node); }
        public virtual void VisitOn(ColumnList node) { }
        public void VisitAfter(ColumnList node)
        {
            Debug.Assert(object.ReferenceEquals(this.Parent, node));
            this.Pop();
        }


        public void VisitBefore(FromClause node) { this.Push(node); }
        public virtual void VisitOn(FromClause node) { }
        public void VisitAfter(FromClause node)
        {
            Debug.Assert(object.ReferenceEquals(this.Parent, node));
            this.Pop();
        }

        public void VisitBefore(RawSql node) { this.Push(node); }
        public virtual void VisitOn(RawSql node) { }
        public void VisitAfter(RawSql node)
        {
            Debug.Assert(object.ReferenceEquals(this.Parent, node));
            this.Pop();
        }


        #endregion
    }
}
