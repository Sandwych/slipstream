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
        void DebugException(string msg, Exception ex);

        void Info(object msg);
        void InfoException(string msg, Exception ex);

        void Warn(object msg);
        void WarnException(string msg, Exception ex);

        void Error(object msg);
        void ErrorException(string msg, Exception ex);

        void Fatal(object msg);
        void FatalException(string msg, Exception ex);

    }
}
