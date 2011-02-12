using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.Model.Query
{
    [Serializable]
    public class TrueExpression : IExpression
    {
        private IExpression[] children;

        public TrueExpression()
        {
            this.children = new IExpression[] { };
        }

        public string Name
        {
            get { return "true"; }
        }

        public IExpression[] Children
        {
            get { return this.children; }
        }

        public string ToSqlString()
        {
            return "true";
        }
    }
}
