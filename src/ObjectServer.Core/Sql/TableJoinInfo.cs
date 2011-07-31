using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NHibernate.SqlCommand;

namespace ObjectServer.Sql
{
    internal class TableJoinInfo
    {
        public TableJoinInfo(string table, string alias, string fkColumn, string pkColumn)
        {
            this.Table = table;
            this.Alias = alias;
            this.FkColumn = fkColumn;
            this.PkColumn = pkColumn;
        }

        public string Table { get; private set; }
        public string Alias { get; private set; }
        public string FkColumn { get; private set; }
        public string PkColumn { get; private set; }
    }
}
