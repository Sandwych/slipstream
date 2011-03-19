using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer
{
    [Serializable]
    public sealed class DatabaseNotFoundException : Exception
    {
        public DatabaseNotFoundException(string msg, string dbName)
            : base(msg)
        {
            this.Database = dbName;
        }

        public string Database { get; private set; }
    }
}
