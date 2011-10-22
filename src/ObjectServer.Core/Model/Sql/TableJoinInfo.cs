using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NHibernate.SqlCommand;

namespace ObjectServer.Model
{
    internal class TableJoinInfo
    {
        public TableJoinInfo(string table, string alias, string fkColumn, string pkColumn)
        {
            if (string.IsNullOrEmpty(table))
            {
                throw new ArgumentNullException("table");
            }

            if (string.IsNullOrEmpty(alias))
            {
                throw new ArgumentNullException("alias");
            }

            if (string.IsNullOrEmpty(fkColumn))
            {
                throw new ArgumentNullException("fkColumn");
            }

            if (string.IsNullOrEmpty(pkColumn))
            {
                throw new ArgumentNullException("pkColumn");
            }

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
