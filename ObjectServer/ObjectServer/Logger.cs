using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace ObjectServer
{
    public static class Logger
    {
        private static readonly log4net.ILog log =
            GetLogger();
        private static readonly bool isDebugEnabled = log.IsDebugEnabled;
        private static readonly bool isInfoEnabled = log.IsInfoEnabled;
        private static readonly bool isWarnEnabled = log.IsWarnEnabled;
        private static readonly bool isErrorEnabled = log.IsErrorEnabled;
        private static readonly bool isFatalEnabled = log.IsFatalEnabled;

        private static log4net.ILog GetLogger()
        {
            var stack = new StackTrace();
            var frame = stack.GetFrame(2);
            return log4net.LogManager.GetLogger(frame.GetMethod().DeclaringType);
        }

        /// <summary>
        /// 为什么这里这些方法的参数都是函数？
        /// 因为如果需要记录日志，那么拼接日志字符串是代价很高的操作，如果不需要记录日志我们应该避免这样的开销
        /// </summary>
        /// <param name="dg"></param>
        public static void Info(Func<string> dg)
        {
            if (isInfoEnabled)
            {
                log.Info(dg());
            }
        }

        public static void Debug(Func<string> dg)
        {
            if (isDebugEnabled)
            {
                log.Info(dg());
            }
        }

        public static void Error(Func<string> dg)
        {
            if (isErrorEnabled)
            {
                log.Error(dg());
            }
        }

        public static void Warn(Func<string> dg)
        {
            if (isWarnEnabled)
            {
                log.Warn(dg());
            }
        }

        public static void Fatal(Func<string> dg)
        {
            if (isFatalEnabled)
            {
                log.Fatal(dg());
            }
        }

        public static void Error(string msg, Exception ex)
        {
            log.Error(msg, ex);
        }

        public static void Fatal(string msg, Exception ex)
        {
            log.Error(msg, ex);
        }
    }
}
