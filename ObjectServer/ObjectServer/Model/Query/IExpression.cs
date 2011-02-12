using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.Model.Query
{
    public interface IExpression
    {
        string Name { get; }
        IExpression[] Children { get; }
       
        string ToSqlString();
    }
}
