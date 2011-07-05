using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer
{
    public abstract class FatalException : Exception
    {
        public FatalException(string msg, Exception innerExp)
            : base(msg, innerExp)
        {
        }
    }
}
