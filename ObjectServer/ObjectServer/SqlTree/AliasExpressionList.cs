using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.SqlTree
{
    public class AliasExpressionList : Node, IExpressionCollection
    {
        private List<AliasExpression> expressions = new List<AliasExpression>();

        public AliasExpressionList(IEnumerable<string> columns)
        {
            this.expressions.Capacity = columns.Count();
            foreach (var f in columns)
            {
                var aliasExp = new AliasExpression(f);
                this.expressions.Add(aliasExp);
            }
        }

        public AliasExpressionList(IEnumerable<AliasExpression> exps)
        {
            this.expressions.AddRange(exps);
        }

        public IList<AliasExpression> Expressions
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
            return new AliasExpressionList(this.Expressions);
        }

        #endregion
    }
}
