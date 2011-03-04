using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer
{
    [Serializable]
    public sealed class BadResourceNameException : Exception
    {
        public BadResourceNameException(string msg, string objName) :
            base(msg)
        {
            this.ObjectName = objName;
        }

        public string ObjectName { get; private set; }
    }
}
