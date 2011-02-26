using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer
{
    [Serializable]
    public sealed class BadServiceMethodException : Exception
    {
        public BadServiceMethodException(string msg, string objName, string methodName) :
            base(msg)
        {
            this.ObjectName = objName;
            this.MethodName = methodName;
        }

        public string ObjectName { get; private set; }
        public string MethodName { get; private set; }
    }
}
