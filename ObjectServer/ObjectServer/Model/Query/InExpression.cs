using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.Model.Query
{
    [Serializable]
    public class InExpression : IExpression
    {
        public InExpression(IExpression exp, params IExpression[] exps)
        {
            IExpression[] children;
            if (exps.Length > 0)
            {
                children = new IExpression[1 + exps.Length];
                children[0] = exp;
                exps.CopyTo(children, 1);
            }
            else
            {
                children = new IExpression[1];
            }
            this.Children = children;
        }

        #region IExpression 成员

        public string Name
        {
            get { return "in"; }
        }

        public IExpression[] Children
        {
            get;
            private set;
        }

        public string ToSqlString()
        {
            return string.Format("({0} in ({1}))" );
        }

        #endregion
    }
}
