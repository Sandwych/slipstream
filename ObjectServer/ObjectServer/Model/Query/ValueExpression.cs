using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.Model.Query
{
    public abstract class ValueExpression : IExpression
    {

        #region IExpression 成员

        public abstract string Name
        {
            get;
        }

        public abstract IExpression[] Children
        {
            get;
        }

        public abstract string ToSqlString();

        #endregion
    }
}
