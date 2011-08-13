using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer
{
    public interface ILogger
    {
        bool IsDebugEnabled { get; }
        bool IsInfoEnabled { get; }
        bool IsWarnEnabled { get; }
        bool IsErrorEnabled { get; }
        bool IsFatalEnabled { get; }

        void Debug(object msg);
        void Debug(Func<object> dg);
        void Debug(object msg, Exception ex);

        void Info(object msg);
        void Info(Func<object> dg);
        void Info(object msg, Exception ex);

        void Warn(object msg);
        void Warn(Func<object> dg);
        void Warn(object msg, Exception ex);

        void Error(object msg);
        void Error(Func<object> dg);
        void Error(object msg, Exception ex);

        void Fatal(object msg);
        void Fatal(Func<object> dg);
        void Fatal(object msg, Exception ex);

    }
}
