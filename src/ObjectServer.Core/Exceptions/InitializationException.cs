using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.Exceptions
{
    /// <summary>
    /// 框架初始化异常
    /// </summary>
    [Serializable]
    public class InitializationException : FatalException
    {
        public InitializationException(string msg, Exception innerExp)
            : base(msg, innerExp)
        {
        }
    }
}
