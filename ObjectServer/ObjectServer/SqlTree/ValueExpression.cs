using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.SqlTree
{
    public class ValueExpression : Node, IExpression
    {
        public ValueExpression(object value)
        {
            this.Value = value;
        }

        public object Value { get; private set; }


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
            return new ValueExpression(this.Value);
        }

        #endregion
    }
}
