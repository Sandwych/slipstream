using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.SqlTree
{
    public static class NodeExtensions
    {

        public static bool IsFirstExpression(this IExpressionCollection coll, IExpression node)
        {
            if (coll.Expressions.Count > 0
                && object.ReferenceEquals(node, coll.Expressions.First()))
            {
                return true;
            }

            return false;
        }

        public static bool IsLastExpression(this IExpressionCollection coll, IExpression node)
        {
            if (coll.Expressions.Count > 0
                && object.ReferenceEquals(node, coll.Expressions[coll.Expressions.Count - 1]))
            {
                return true;
            }

            return false;
        }

    }
}
