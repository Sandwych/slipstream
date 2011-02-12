using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.Model.Query
{
    [Serializable]
    public class FieldExpression : IExpression
    {
        private string field;

        public FieldExpression(string field)
        {
            this.Children = new IExpression[] { };
            this.field = field;
        }

        #region IExpression 成员

        public string Name
        {
            get { return "field"; }
        }

        public IExpression[] Children
        {
            get;
            private set;
        }

        public string ToSqlString()
        {
            return field;
        }

        #endregion
    }
}
