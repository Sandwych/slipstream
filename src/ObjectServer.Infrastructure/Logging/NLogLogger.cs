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

        public void DebugException(string msg, Exception ex)
        {
            if (this.IsDebugEnabled)
            {
                this.log.DebugException(msg, ex);
            }
        }

        public void Info(object msg)
        {
            if (this.IsInfoEnabled)
            {
                this.log.Info(msg);
            }
        }

        public void InfoException(string msg, Exception ex)
        {
            if (this.IsInfoEnabled)
            {
                this.log.InfoException(msg, ex);
            }
        }

        public void Warn(object msg)
        {
            if (this.IsWarnEnabled)
            {
                this.log.Warn(msg.ToString());
            }
        }

        public void WarnException(string msg, Exception ex)
        {
            if (this.IsWarnEnabled)
            {
                this.log.WarnException(msg, ex);
            }
        }

        public void Error(object msg)
        {
            if (this.IsErrorEnabled)
            {
                this.log.Error(msg);
            }
        }

        public void ErrorException(string msg, Exception ex)
        {
            if (this.IsErrorEnabled)
            {
                this.log.ErrorException(msg, ex);
            }
        }

        public void Fatal(object msg)
        {
            if (this.IsFatalEnabled)
            {
                this.log.Fatal(msg);
            }
        }

        public void FatalException(string msg, Exception ex)
        {
            if (this.IsFatalEnabled)
            {
                this.log.FatalException(msg, ex);
            }
        }
    }
}
