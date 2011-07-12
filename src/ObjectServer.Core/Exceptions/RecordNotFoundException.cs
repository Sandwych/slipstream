using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer
{
    [Serializable]
    public sealed class RecordNotFoundException : Exception
    {
        public RecordNotFoundException(string msg, string resourceName)
            : base(msg)
        {
            this.ResourceName = resourceName;
        }

        public string ResourceName { get; private set; }
    }
}
