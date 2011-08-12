using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using ObjectServer.Model;

namespace ObjectServer.Exceptions
{
    [Serializable]
    public class ValidationException : DataException
    {
        public ValidationException(string msg, IDictionary<string, string> fieldsMsg)
            : base(msg)
        {
            Debug.Assert(fieldsMsg != null);
            this.Fields = fieldsMsg;
        }

        public IDictionary<string, string> Fields { get; private set; }
    }
}
