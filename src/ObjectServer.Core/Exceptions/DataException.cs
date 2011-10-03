using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace ObjectServer.Exceptions
{

    /// <summary>
    /// 跟数据相关的异常
    /// 比如查询结果不正确，或者是系统数据处于不一致的状态
    /// </summary>
    [Serializable]
    public class DataException : Exception
    {
        public DataException(string msg)
            : base(msg)
        {
        }

        public DataException(string msg, Exception innerEx)
            : base(msg, innerEx)
        {
        }

        protected DataException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

    }
}
