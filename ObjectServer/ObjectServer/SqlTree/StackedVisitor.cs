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

        /// <summary>
        /// TODO: 这里是错误的
        /// </summary>
        public INode Parent
        {
            get
            {
                return this.Path.Peek();
            }
        }

        #region IVisitor 成员

        public virtual void VisitBefore(IdentifierExpression node) { this.Push(node); }
        public virtual void VisitOn(IdentifierExpression node) { }
        public virtual void VisitAfter(IdentifierExpression node)
        {
            Debug.Assert(object.ReferenceEquals(this.Parent, node));
            this.Pop();
        }

        public virtual void VisitBefore(SelectStatement node) { this.Push(node); }
        public virtual void VisitOn(SelectStatement node) { }
        public virtual void VisitAfter(SelectStatement node)
        {
            Debug.Assert(object.ReferenceEquals(this.Parent, node));
            this.Pop();
        }

        public virtual void VisitBefore(ExpressionList node) { this.Push(node); }
        public virtual void VisitOn(ExpressionList node) { }
        public virtual void VisitAfter(ExpressionList node)
        {
            Debug.Assert(object.ReferenceEquals(this.Parent, node));
            this.Pop();
        }


        public virtual void VisitBefore(FromClause node) { this.Push(node); }
        public virtual void VisitOn(FromClause node) { }
        public virtual void VisitAfter(FromClause node)
        {
            Debug.Assert(object.ReferenceEquals(this.Parent, node));
            this.Pop();
        }


        public virtual void VisitBefore(WhereClause node) { this.Push(node); }
        public virtual void VisitOn(WhereClause node) { }
        public virtual void VisitAfter(WhereClause node)
        {
            Debug.Assert(object.ReferenceEquals(this.Parent, node));
            this.Pop();
        }


        public virtual void VisitBefore(JoinClause node) { this.Push(node); }
        public virtual void VisitOn(JoinClause node) { }
        public virtual void VisitAfter(JoinClause node)
        {
            Debug.Assert(object.ReferenceEquals(this.Parent, node));
            this.Pop();
        }

        public virtual void VisitBefore(RawSql node) { this.Push(node); }
        public virtual void VisitOn(RawSql node) { }
        public virtual void VisitAfter(RawSql node)
        {
            Debug.Assert(object.ReferenceEquals(this.Parent, node));
            this.Pop();
        }

        public virtual void VisitBefore(UnaryExpression node) { this.Push(node); }
        public virtual void VisitOn(UnaryExpression node) { }
        public virtual void VisitAfter(UnaryExpression node)
        {
            Debug.Assert(object.ReferenceEquals(this.Parent, node));
            this.Pop();
        }

        public virtual void VisitBefore(BinaryExpression node) { this.Push(node); }
        public virtual void VisitOn(BinaryExpression node) { }
        public virtual void VisitAfter(BinaryExpression node)
        {
            Debug.Assert(object.ReferenceEquals(this.Parent, node));
            this.Pop();
        }

        public virtual void VisitBefore(ExpressionOperator node) { this.Push(node); }
        public virtual void VisitOn(ExpressionOperator node) { }
        public virtual void VisitAfter(ExpressionOperator node)
        {
            Debug.Assert(object.ReferenceEquals(this.Parent, node));
            this.Pop();
        }

        public virtual void VisitBefore(ValueExpression node) { this.Push(node); }
        public virtual void VisitOn(ValueExpression node) { }
        public virtual void VisitAfter(ValueExpression node)
        {
            Debug.Assert(object.ReferenceEquals(this.Parent, node));
            this.Pop();
        }

        public virtual void VisitBefore(AliasExpression node) { this.Push(node); }
        public virtual void VisitOn(AliasExpression node) { }
        public virtual void VisitAfter(AliasExpression node)
        {
            Debug.Assert(object.ReferenceEquals(this.Parent, node));
            this.Pop();
        }


        #endregion
    }
}
