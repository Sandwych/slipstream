using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.Model.Query
{
    [Serializable]
    public class NotExpression : IExpression
    {
        public NotExpression(IExpression exp)
        {
            this.Children = new IExpression[] { exp };
        }

        #region IExpression 成员

        public string Name
        {
            get { return "not"; }
        }

        public IExpression[] Children
        {
            get;
            private set;
        }

        public string ToSqlString()
        {
            return string.Format("(not {0})");
        }

        #endregion
    }
}
