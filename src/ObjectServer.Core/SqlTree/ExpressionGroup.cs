using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;

namespace ObjectServer.SqlTree
{
    public class ExpressionGroup : Node, IExpressionCollection
    {
        private List<IExpression> expressions = new List<IExpression>();

        public ExpressionGroup(IEnumerable<object> values)
        {
            this.expressions.Capacity = values.Count();
            foreach (var v in values)
            {
                this.expressions.Add(new ValueExpression(v));
            }
        }

        public ExpressionGroup(IEnumerable values)
        {
            foreach (var v in values)
            {
                this.expressions.Add(new ValueExpression(v));
            }
        }

        public ExpressionGroup(IEnumerable<string> columns)
        {
            this.expressions.Capacity = columns.Count();
            foreach (var f in columns)
            {
                var aliasExp = new AliasExpression(f);
                this.expressions.Add(aliasExp);
            }
        }

        public ExpressionGroup(IEnumerable<IExpression> exps)
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
            return new ExpressionGroup(this.Expressions);
        }

        #endregion
    }
}
