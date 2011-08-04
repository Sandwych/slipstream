using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.Exceptions
{
    [Serializable]
    public sealed class BadFieldNameException : ResourceException
    {
        public BadFieldNameException(string msg, string fieldName) :
            base(msg)
        {
            this.FieldName = fieldName;
        }

        public string FieldName { get; private set; }
    }
}
