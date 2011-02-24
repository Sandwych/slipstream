
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.SqlTree
{
    public class FromClause : Node, IClause
    {
        public FromClause(IEnumerable<string> tokens)
        {
            var n = tokens.Count();
            var exps = new IExpression[n];
            var i = 0;
            foreach (var tok in tokens)
            {
                exps[i] = new IdentifierExpression(tok);
                i++;
            }

            var expColl = new ExpressionList(exps);
            this.ExpressionCollection = expColl;
        }

        public FromClause(ExpressionList exp)
        {
            this.ExpressionCollection = exp;
        }

        public FromClause(AliasExpression aliasExp)
        {
            var aliasExps = new AliasExpression[] { aliasExp };
            this.ExpressionCollection = new ExpressionList(aliasExps);
        }

        public ExpressionList ExpressionCollection { get; private set; }

        #region INode 成员

        public override void Traverse(IVisitor visitor)
        {
            visitor.VisitBefore(this);
            visitor.VisitOn(this);
            this.ExpressionCollection.Traverse(visitor);
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
