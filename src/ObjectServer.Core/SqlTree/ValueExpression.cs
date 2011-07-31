using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.SqlTree
{
    public class ValueExpression : Node, IExpression
    {
        private static readonly ValueExpression s_trueExp = new ValueExpression(true);
        private static readonly ValueExpression s_falseExp = new ValueExpression(false);
        private static readonly ValueExpression s_nullExp = new ValueExpression(null);

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

        public static IExpression TrueExpression { get { return s_trueExp; } }
        public static IExpression FalseExpression { get { return s_falseExp; } }
        public static IExpression NullExpression { get { return s_nullExp; } }

        public override string ToString()
        {
            if (!this.Value.IsNull())
            {
                return this.Value.ToString();
            }
            else
            {
                return "NULL";
            }
        }
    }
}
