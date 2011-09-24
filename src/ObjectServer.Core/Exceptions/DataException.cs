using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace ObjectServer.Exceptions
{

    /// <summary>
    /// 跟数据库查询相关的异常
    /// </summary>
    [Serializable]
    public class DataException : Exception
    {
        public DataException(string msg)
            : base(msg)
        {
        }

        protected DataException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

    }
}
