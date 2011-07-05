using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.SqlTree
{
    public class AliasExpressionList : Node, IExpressionCollection
    {
        private List<IExpression> expressions = new List<IExpression>();

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

        public IList<IExpression> Expressions
        {
            get { return this.expressions; }
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
            throw new NotImplementedException();
        }

        #endregion
    }
}
