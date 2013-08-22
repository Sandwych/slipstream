using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SlipStream.Exceptions
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
