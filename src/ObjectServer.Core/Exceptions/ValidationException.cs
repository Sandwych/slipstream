using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ObjectServer.Model;

namespace ObjectServer
{
    [Serializable]
    public class ValidationException : Exception
    {
        public ValidationException(string msg)
            : base(msg)
        {
        }
    }
}
