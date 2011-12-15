using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer
{
    public static class LoggerExtensions
    {
        public static void Debug(this ILogger self, Func<string> msgproc)
        {
            if (self.IsDebugEnabled)
            {
                self.Debug(msgproc());
            }
        }

        public static void DebugFormat(this ILogger self, string format, params object[] args)
        {
            if (self.IsDebugEnabled)
            {
                self.Debug(String.Format(format, args));
            }
        }

        public static void Info(this ILogger self, Func<string> msgproc)
        {
            if (self.IsInfoEnabled)
            {
                self.Info(msgproc());
            }
        }

        public static void InfoFormat(this ILogger self, string format, params object[] args)
        {
            if (self.IsInfoEnabled)
            {
                self.Info(String.Format(format, args));
            }
        }

        public static void Warn(this ILogger self, Func<string> msgproc)
        {
            if (self.IsWarnEnabled)
            {
                self.Warn(msgproc());
            }
        }

        public static void WarnFormat(this ILogger self, string format, params object[] args)
        {
            if (self.IsWarnEnabled)
            {
                self.Warn(String.Format(format, args));
            }
        }

        public static void Error(this ILogger self, Func<string> msgproc)
        {
            if (self.IsErrorEnabled)
            {
                self.Error(msgproc());
            }
        }

        public static void ErrorFormat(this ILogger self, string format, params object[] args)
        {
            if (self.IsErrorEnabled)
            {
                self.Error(String.Format(format, args));
            }
        }

        public static void Fatal(this ILogger self, Func<string> msgproc)
        {
            if (self.IsFatalEnabled)
            {
                self.Fatal(msgproc());
            }
        }

        public static void FatalFormat(this ILogger self, string format, params object[] args)
        {
            if (self.IsFatalEnabled)
            {
                self.Fatal(String.Format(format, args));
            }
        }
    }
}
