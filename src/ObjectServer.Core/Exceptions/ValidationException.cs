using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ObjectServer.Model;

namespace ObjectServer.Exceptions
{
    [Serializable]
    public class ValidationException : DataException
    {
        public ValidationException(string msg)
            : base(msg)
        {
        }
    }
}
