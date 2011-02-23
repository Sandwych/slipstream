using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.SqlTree
{
    public class ExpressionList : Node, IExpression
    {
        private List<IExpression> expressions = new List<IExpression>();

        public ExpressionList(IEnumerable<string> columns)
        {
            this.expressions.Capacity = columns.Count();
            foreach (var f in columns)
            {
                var aliasExp = new AliasExpression(f);
                this.expressions.Add(aliasExp);
            }
        }

        public ExpressionList(IEnumerable<IExpression> exps)
        {
            this.expressions.AddRange(exps);
        }

        public IList<IExpression> Expressions
        {
            get { return this.expressions; }
        }

        public bool IsFirstExpression(IExpression node)
        {
            if (this.expressions.Count > 0
                && object.ReferenceEquals(node, this.expressions[0]))
            {
                return true;
            }

            return false;
        }

        public bool IsLastExpression(IExpression node)
        {
            if (this.expressions.Count > 0
                && object.ReferenceEquals(node, this.expressions[this.expressions.Count - 1]))
            {
                return true;
            }

            return false;
        }


        #region INode 成员

        public override void Traverse(IVisitor visitor)
        {
            visitor.VisitBefore(this);
            visitor.VisitOn(this);
            foreach (var exp in this.expressions)
            {
                exp.Traverse(visitor);
            }

            visitor.VisitAfter(this);
        }

        #endregion

        #region ICloneable 成员

        public override object Clone()
        {
            return new ExpressionList(this.Expressions);
        }

        #endregion
    }
}
