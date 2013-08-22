using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace SlipStream.Exceptions
{
    /// <summary>
    /// 表示系统中发生的致命错误
    /// </summary>
    public abstract class FatalException : Exception
    {
        protected FatalException(string msg, Exception innerExp)
            : base(msg, innerExp)
        {
        }

        protected FatalException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
