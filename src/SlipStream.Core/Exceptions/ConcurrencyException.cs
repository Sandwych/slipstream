using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SlipStream.Exceptions
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
