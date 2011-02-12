using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.Model.Query
{
    [Serializable]
    public class EqualExpression : IExpression
    {
        public EqualExpression(IExpression lhs, IExpression rhs)
        {
            this.Children = new IExpression[2] { lhs, rhs };
        }

        #region IExpression 成员

        public string Name
        {
            get { return "="; }
        }

        public IExpression[] Children
        {
            get;
            private set;
        }

        public string ToSqlString()
        {
            return string.Format("({0} = {1})",
                this.Children[0].ToSqlString(),
                this.Children[1].ToSqlString());
        }

        #endregion
    }
}
