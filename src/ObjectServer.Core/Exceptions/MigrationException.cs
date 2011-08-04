using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.Exceptions
{
    [Serializable]
    public sealed class MigrationException : DataException
    {
        public MigrationException(string msg)
            : base(msg)
        {
        }

    }
}
