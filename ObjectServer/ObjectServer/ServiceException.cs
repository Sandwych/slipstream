using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer
{
    /// <summary>
    /// 服务异常类，这个类用于抛给客户端表示服务方法调用失败
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ServiceException<T> : Exception
    {
        public ServiceException(T innerException)
        {
        }
    }
}
