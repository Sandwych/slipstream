using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SlipStream.Exceptions
{
    [Serializable]
    public sealed class DatabaseNotFoundException : ResourceException
    {
        public DatabaseNotFoundException(string msg, string dbName)
            : base(msg)
        {
            this.Database = dbName;
        }

        public string Database { get; private set; }
    }
}
