using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace ObjectServer.SqlTree
{
    public static class ExpressionExtensions
    {
        public static IExpression JoinExpressions(this IList<IExpression> expressions, ExpressionOperator opr)
        {
            if (opr == null)
            {
                throw new ArgumentNullException("opr");
            }
            if (expressions == null)
            {
                throw new ArgumentNullException("expressions");
            }
            if (expressions.Count <= 0)
            {
                throw new ArgumentOutOfRangeException("expressions");
            }

            IExpression expTop;
            int expCount = expressions.Count;

            expTop = expressions.Last();

            for (int i = expCount - 2; i >= 0; i--)
            {
                var rhs = expTop;
                var andExp = new BinaryExpression(expressions[i], opr, rhs);
                expTop = andExp;
            }
            return expTop;
        }

    }
}
