using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.SqlTree
{
    public interface IComposedExpression : IExpression
    {
        IExpression Next { get; }
    }
}
