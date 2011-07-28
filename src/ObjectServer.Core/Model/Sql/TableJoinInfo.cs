using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ObjectServer.SqlTree;

namespace ObjectServer.Model.Sql
{
    internal class TableJoinInfo
    {
        public TableJoinInfo(string table, string alias, IExpression restriction)
        {
            this.Table = table;
            this.Alias = alias;
            this.Restriction = restriction;
        }

        public string Table { get; private set; }
        public string Alias { get; private set; }
        public IExpression Restriction { get; private set; }
    }
}
