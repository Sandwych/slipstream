using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.SqlTree
{
    public sealed class ParameterExpression : Node, IExpression
    {
        public ParameterExpression(string param)
        {
            this.Parameter = param;
        }

        public string Parameter { get; private set; }


        #region INode 成员

        public override void Traverse(IVisitor visitor)
        {
            visitor.VisitBefore(this);
            visitor.VisitOn(this);
            visitor.VisitAfter(this);
        }

        #endregion

        #region ICloneable 成员

        public override object Clone()
        {
            return new ParameterExpression(this.Parameter);
        }

        #endregion
    }
}
