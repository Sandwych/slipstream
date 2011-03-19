using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.SqlTree
{
    public interface IBinaryExpression : IExpression
    {
        IExpression Lhs { get; set; }
        IExpression Rhs { get; set; }
        ExpressionOperator Operator { get; set; }
    }
}
