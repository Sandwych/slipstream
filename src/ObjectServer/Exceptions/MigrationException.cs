using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer
{
    [Serializable]
    public sealed class MigrationException : Exception
    {
        public MigrationException(string msg)
            : base(msg)
        {
        }

    }
}
