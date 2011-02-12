using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.Model.Query
{
    [Serializable]
    public class StringValueExpression : ValueExpression
    {
        private IExpression[] children;

        public StringValueExpression(string value)
        {
            this.Value = value;
            this.children = new IExpression[] { };
        }

        public string Value { get; private set; }

        public override string Name
        {
            get { return "string"; }
        }

        public override IExpression[] Children
        {
            get { return this.children; }
        }

        public override string ToSqlString()
        {
            return "'" + this.Value.Replace("'", "''") + "'";
        }
    }
}
