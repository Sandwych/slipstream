using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer
{
    [Serializable]
    public class ConcurrencyException : Exception
    {
        public ConcurrencyException(string msg)
            : base(msg)
        {
        }
    }
}
