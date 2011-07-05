using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.SqlTree
{
    public class ExpressionOperator : Node
    {
        private static readonly ExpressionOperator s_orOpr = new ExpressionOperator("OR");
        private static readonly ExpressionOperator s_andOpr = new ExpressionOperator("AND");
        private static readonly ExpressionOperator s_notOpr = new ExpressionOperator("NOT");
        private static readonly ExpressionOperator s_equalOpr = new ExpressionOperator("=");
        private static readonly ExpressionOperator s_notEqualOpr = new ExpressionOperator("<>");
        private static readonly ExpressionOperator s_greaterOpr = new ExpressionOperator(">");
        private static readonly ExpressionOperator s_greaterEqualOpr = new ExpressionOperator(">=");
        private static readonly ExpressionOperator s_lessOpr = new ExpressionOperator("<");
        private static readonly ExpressionOperator s_lessEqualOpr = new ExpressionOperator("<=");
        private static readonly ExpressionOperator s_likeOpr = new ExpressionOperator("LIKE");
        private static readonly ExpressionOperator s_notLikeOpr = new ExpressionOperator("NOT LIKE");
        private static readonly ExpressionOperator s_inOpr = new ExpressionOperator("IN");
        private static readonly ExpressionOperator s_notInOpr = new ExpressionOperator("NOT IN");


        public ExpressionOperator(string opr)
        {
            this.Operator = opr;
        }

        public string Operator { get; private set; }

        public static ExpressionOperator OrOperator { get { return s_orOpr; } }
        public static ExpressionOperator AndOperator { get { return s_andOpr; } }
        public static ExpressionOperator NotOperator { get { return s_notOpr; } }
        public static ExpressionOperator EqualOperator { get { return s_equalOpr; } }
        public static ExpressionOperator NotEqualOperator { get { return s_notEqualOpr; } }
        public static ExpressionOperator LikeOperator { get { return s_likeOpr; } }
        public static ExpressionOperator NotLikeOperator { get { return s_notLikeOpr; } }
        public static ExpressionOperator InOperator { get { return s_inOpr; } }
        public static ExpressionOperator NotInOperator { get { return s_notInOpr; } }
        public static ExpressionOperator LessOperator { get { return s_lessOpr; } }
        public static ExpressionOperator LessEqualOperator { get { return s_lessEqualOpr; } }
        public static ExpressionOperator GreaterOperator { get { return s_greaterOpr; } }
        public static ExpressionOperator GreaterEqualOperator { get { return s_greaterEqualOpr; } }

        public override void Traverse(IVisitor visitor)
        {
            visitor.VisitBefore(this);
            visitor.VisitOn(this);
            visitor.VisitAfter(this);
        }

        public override object Clone()
        {
            return new ExpressionOperator(this.Operator);
        }

        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(this, obj))
            {
                return true;
            }
            var rhs = (ExpressionOperator)obj;
            return this.Operator == rhs.Operator;
        }

        public override int GetHashCode()
        {
            return this.Operator.GetHashCode();
        }

    }
}
