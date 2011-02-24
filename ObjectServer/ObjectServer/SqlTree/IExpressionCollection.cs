using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.SqlTree
{
    public interface IExpressionCollection : IExpression
    {
        bool IsFirstExpression(IExpression node);
        bool IsLastExpression(IExpression node);
    }
}
