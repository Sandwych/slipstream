using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace ObjectServer
{
    internal sealed class NLogLogger : ILogger
    {
        private readonly NLog.Logger log;

        public NLogLogger(NLog.Logger log)
        {
            System.Diagnostics.Debug.Assert(log != null);

            this.log = log;
        }

        public bool IsDebugEnabled { get { return this.log.IsDebugEnabled; } }
        public bool IsInfoEnabled { get { return this.log.IsInfoEnabled; } }
        public bool IsWarnEnabled { get { return this.log.IsWarnEnabled; } }
        public bool IsErrorEnabled { get { return this.log.IsErrorEnabled; } }
        public bool IsFatalEnabled { get { return this.log.IsFatalEnabled; } }

        public void Debug(object msg)
        {
            if (this.IsDebugEnabled)
            {
                this.log.Debug(msg);
            }
        }

        public void Debug(Func<object> dg)
        {
            if (this.IsDebugEnabled)
            {
                this.log.Debug(dg());
            }
        }

        public void Info(object msg)
        {
            if (this.IsInfoEnabled)
            {
                this.log.Info(msg);
            }
        }

        public void Info(Func<object> dg)
        {
            if (this.IsInfoEnabled)
            {
                this.log.Info(dg());
            }
        }

        public void Warn(object msg)
        {
            if (this.IsWarnEnabled)
            {
                this.log.Warn(msg);
            }
        }

        public void Warn(Func<object> dg)
        {
            if (this.IsWarnEnabled)
            {
                this.log.Warn(dg());
            }
        }

        public void Error(object msg)
        {
            if (this.IsErrorEnabled)
            {
                this.log.Error(msg);
            }
        }

        public void Error(Func<object> dg)
        {
            if (this.IsErrorEnabled)
            {
                this.log.Error(dg());
            }
        }

        public void Error(object msg, Exception ex)
        {
            if (this.IsErrorEnabled)
            {
                this.log.ErrorException(msg.ToString(), ex);
            }
        }

        public void ErrorFormat(string format, params object[] args)
        {
            if (this.IsErrorEnabled)
            {
                var msg = string.Format(format, args);
                this.log.Error(msg);
            }
        }

        public void Fatal(object msg)
        {
            if (this.IsFatalEnabled)
            {
                this.log.Fatal(msg);
            }
        }

        public void Fatal(Func<object> dg)
        {
            if (this.IsFatalEnabled)
            {
                this.log.Fatal(dg());
            }
        }

        public void Fatal(object msg, Exception ex)
        {
            if (this.IsFatalEnabled)
            {
                this.log.FatalException(msg.ToString(), ex);
            }
        }
    }
}
