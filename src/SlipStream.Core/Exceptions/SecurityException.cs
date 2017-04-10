using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using SlipStream.Entity;

namespace SlipStream.Exceptions
{
    [Serializable]
    public class SecurityException : Exception
    {
        public SecurityException(string msg)
            : base(msg)
        {
        }

    }
}
