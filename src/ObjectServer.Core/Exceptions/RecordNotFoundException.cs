using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.Exceptions
{
    [Serializable]
    public sealed class RecordNotFoundException : DataException
    {
        public RecordNotFoundException(string msg, string resourceName)
            : base(msg)
        {
            this.ResourceName = resourceName;
        }

        public string ResourceName { get; private set; }
    }
}
