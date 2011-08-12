using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;

using log4net;
using log4net.Appender;
using log4net.Layout;
using log4net.Core;

namespace ObjectServer
{
    public static class LoggerProvider
    {
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
            if (dg == null)
            {
                throw new ArgumentNullException("dg");
            }

            var log = GetLogger();
            if (log.IsInfoEnabled)
            {
                log.Info(dg());
            }
        }

        public static void Debug(Func<string> dg)
        {
            if (dg == null)
            {
                throw new ArgumentNullException("dg");
            }

            var log = GetLogger();
            if (log.IsDebugEnabled)
            {
                log.Debug(dg());
            }
        }

        public static void Error(Func<string> dg)
        {
            if (dg == null)
            {
                throw new ArgumentNullException("dg");
            }

            var log = GetLogger();
            if (log.IsErrorEnabled)
            {
                log.Error(dg());
            }
        }

        public static void Warn(Func<string> dg)
        {
            if (dg == null)
            {
                throw new ArgumentNullException("dg");
            }

            var log = GetLogger();
            if (log.IsWarnEnabled)
            {
                log.Warn(dg());
            }
        }

        public static void Fatal(Func<string> dg)
        {
            if (dg == null)
            {
                throw new ArgumentNullException("dg");
            }

            var log = GetLogger();
            if (log.IsFatalEnabled)
            {
                log.Fatal(dg());
            }
        }

        public static void Error(string msg, Exception ex)
        {
            if (string.IsNullOrEmpty(msg))
            {
                throw new ArgumentNullException("msg");
            }

            if (ex == null)
            {
                throw new ArgumentNullException("ex");
            }

            var log = GetLogger();
            if (log.IsErrorEnabled)
            {
                log.Error(msg, ex);
            }
        }

        public static void Fatal(string msg, Exception ex)
        {
            var log = GetLogger();
            if (log.IsFatalEnabled)
            {
                log.Fatal(msg, ex);
            }
        }

        public static ObjectServer.ILogger PlatformLogger
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public static ObjectServer.ILogger BizLogger
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public static void Configurate(Config cfg)
        {
            if (cfg == null)
            {
                throw new ArgumentNullException("cfg");
            }

            var layout = new PatternLayout(StaticSettings.LogPattern);

            if (!string.IsNullOrEmpty(cfg.LogPath))
            {
                var logPath = Path.Combine(cfg.LogPath, StaticSettings.LogFileName);
                var mainFileAppender = CreateRollingFileAppender(layout, logPath);

                log4net.Config.BasicConfigurator.Configure(mainFileAppender);
            }

            var hierarchy = (log4net.Repository.Hierarchy.Hierarchy)LogManager.GetRepository();

            if (cfg.Debug)
            {
                hierarchy.Root.Level = log4net.Core.Level.Debug;
                var traceAppender = CreateTraceAppender(layout);
                hierarchy.Root.AddAppender(traceAppender);
            }
            else
            {
                hierarchy.Root.Level = log4net.Core.Level.Info;
            }

            hierarchy.Configured = true;

            /*
            TurnOnLogging();
             */
        }

        private static void AddAppender(string loggerName, IAppender appender)
        {
            System.Diagnostics.Debug.Assert(!string.IsNullOrEmpty(loggerName));
            System.Diagnostics.Debug.Assert(appender != null);

            var log = LogManager.GetLogger(loggerName);
            var l = (log4net.Repository.Hierarchy.Logger)log.Logger;
            l.AddAppender(appender);
        }

        private static void SetLevel(string loggerName, string levelName)
        {
            System.Diagnostics.Debug.Assert(!string.IsNullOrEmpty(loggerName));
            System.Diagnostics.Debug.Assert(!string.IsNullOrEmpty(levelName));

            //TODO 检查 levelName

            var log = LogManager.GetLogger(loggerName);
            var l = (log4net.Repository.Hierarchy.Logger)log.Logger;

            l.Level = l.Hierarchy.LevelMap[levelName];
        }

        private static IAppender CreateRollingFileAppender(PatternLayout layout, string logPath)
        {
            if (!System.IO.Directory.Exists(logPath))
            {
                throw new DirectoryNotFoundException(logPath);
            }

            var fileAppender = new log4net.Appender.RollingFileAppender()
            {
                File = logPath,
                AppendToFile = true,
                RollingStyle = log4net.Appender.RollingFileAppender.RollingMode.Size,
                Layout = layout,
                Encoding = Encoding.UTF8,
                StaticLogFileName = true,
            };
            fileAppender.ActivateOptions();
            return fileAppender;
        }

        private static IAppender CreateTraceAppender(PatternLayout layout)
        {
            var ta = new TraceAppender()
            {
                Layout = layout,
            };
            ta.ActivateOptions();
            return ta;
        }

        private static IAppender CreateColoredConsoleAppender(PatternLayout layout)
        {
            var cca = new ColoredConsoleAppender()
            {
                Layout = layout,
            };

            var errorColorMapping = new ColoredConsoleAppender.LevelColors()
            {
                Level = Level.Error,
                ForeColor = ColoredConsoleAppender.Colors.Red,
            };

            var debugColorMapping = new ColoredConsoleAppender.LevelColors()
            {
                Level = Level.Debug,
                ForeColor = ColoredConsoleAppender.Colors.White,
            };

            var fatalColorMapping = new ColoredConsoleAppender.LevelColors()
            {
                Level = Level.Fatal,
                ForeColor = ColoredConsoleAppender.Colors.Red,
            };

            var warnColorMapping = new ColoredConsoleAppender.LevelColors()
            {
                Level = Level.Fatal,
                ForeColor = ColoredConsoleAppender.Colors.Yellow,
            };

            cca.AddMapping(errorColorMapping);
            cca.AddMapping(fatalColorMapping);
            cca.AddMapping(debugColorMapping);
            cca.AddMapping(warnColorMapping);

            cca.ActivateOptions();
            return cca;
        }
    }
}
