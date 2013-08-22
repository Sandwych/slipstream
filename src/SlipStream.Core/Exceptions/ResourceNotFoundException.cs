using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SlipStream.Exceptions
{
    [Serializable]
    public sealed class ResourceNotFoundException : ResourceException
    {
        public ResourceNotFoundException(string msg, string resName)
            : base(msg)
        {
            this.ResourceName = resName;
        }

        public string ResourceName { get; private set; }
    }
}
