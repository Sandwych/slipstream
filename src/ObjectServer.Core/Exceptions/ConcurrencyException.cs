using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.Exceptions
{
    [Serializable]
    public class ConcurrencyException : DataException
    {
        public ConcurrencyException(string msg)
            : base(msg)
        {
        }
    }
}
