using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;

namespace ObjectServer
{
    public sealed class LoggerProvider
    {
        public const string EnvironmentLoggerName = "environment";
        public const string BizLoggerName = "biz";
        public const string RpcLoggerName = "rpc";

        private const string LogArchiveDirectoryName = "archives";
        private const string LoggingLayout = "[${longdate}|${level:uppercase=true}|${logger}]${message}";

        public static readonly Dictionary<string, NLog.LogLevel> LogLevelMapping =
            new Dictionary<string, NLog.LogLevel>()
            {
                { "all", NLog.LogLevel.Trace },
                { "debug", NLog.LogLevel.Debug },
                { "info", NLog.LogLevel.Info },
                { "warn", NLog.LogLevel.Warn },
                { "error", NLog.LogLevel.Error },
                { "fatal", NLog.LogLevel.Fatal },
            };

        private readonly static LoggerProvider s_instance = new LoggerProvider();
        private readonly NLogLogger envLogger;
        private readonly NLogLogger bizLogger;
        private readonly NLogLogger rpcLogger;

        public LoggerProvider()
        {
            var platformLog = NLog.LogManager.GetLogger(EnvironmentLoggerName);
            this.envLogger = new NLogLogger(platformLog);

            var bizLog = NLog.LogManager.GetLogger(BizLoggerName);
            this.bizLogger = new NLogLogger(bizLog);

            var rpcLog = NLog.LogManager.GetLogger(RpcLoggerName);
            this.rpcLogger = new NLogLogger(rpcLog);
        }

        public static ObjectServer.ILogger EnvironmentLogger
        {
            get
            {
                return s_instance.envLogger;
            }
        }

        public static ObjectServer.ILogger BizLogger
        {
            get
            {
                return s_instance.bizLogger;
            }
        }

        public static ObjectServer.ILogger RpcLogger
        {
            get
            {
                return s_instance.rpcLogger;
            }
        }

        public static void Configurate(ShellSettings cfg)
        {
            if (cfg == null)
            {
                throw new ArgumentNullException("cfg");
            }

            EnsureLoggingPathExist(cfg);

            var logCfg = new NLog.Config.LoggingConfiguration();

            if (cfg.Debug)
            {
                ConfigurateConsoleLogger(logCfg);
                ConfigurateDebuggerLogger(logCfg);
            }

            if (!string.IsNullOrEmpty(cfg.LogPath))
            {
                var logDir = Environment.ExpandEnvironmentVariables(cfg.LogPath);

                ConfigurateFileLogger(logDir, logCfg, EnvironmentLoggerName);
                ConfigurateFileLogger(logDir, logCfg, RpcLoggerName);
                ConfigurateFileLogger(logDir, logCfg, BizLoggerName);
            }

            NLog.LogManager.Configuration = logCfg;
        }

        private static void ConfigurateFileLogger(string logDir, NLog.Config.LoggingConfiguration logCfg, string loggerName)
        {
            var fileName = Path.Combine(logDir, loggerName + ".log");
            var fileTarget = CreateFileTarget(loggerName, fileName);
            logCfg.AddTarget(loggerName, fileTarget);
            var rule = new NLog.Config.LoggingRule(loggerName, NLog.LogLevel.Trace, fileTarget);
            logCfg.LoggingRules.Add(rule);
        }

        private static NLog.Targets.FileTarget CreateFileTarget(string targetName, string filePath)
        {
            Debug.Assert(!string.IsNullOrEmpty(filePath));
            var fileName = Path.GetFileName(filePath);
            var archiveDir = Path.Combine(Path.GetDirectoryName(filePath), LogArchiveDirectoryName);
            var archiveFile = Path.Combine(archiveDir, fileName);

            var target = new NLog.Targets.FileTarget();
            target.Layout = LoggingLayout;
            target.FileName = filePath;
            target.ArchiveFileName = archiveFile + ".{#####}";
            target.ArchiveAboveSize = 4 * 1024 * 1024; // archive files greater than 4MB
            target.ArchiveNumbering = NLog.Targets.ArchiveNumberingMode.Sequence;

            // this speeds up things when no other processes are writing to the file
            target.ConcurrentWrites = true;

            return target;
        }

        private static void ConfigurateConsoleLogger(NLog.Config.LoggingConfiguration logCfg)
        {
            Debug.Assert(logCfg != null);

            var consoleTarget = new NLog.Targets.ColoredConsoleTarget()
            {
                Name = "console",
                Layout = LoggingLayout,
            };

            logCfg.AddTarget("console", consoleTarget);

            var rule1 = new NLog.Config.LoggingRule("*", NLog.LogLevel.Trace, consoleTarget);
            logCfg.LoggingRules.Add(rule1);
        }

        private static void ConfigurateDebuggerLogger(NLog.Config.LoggingConfiguration logCfg)
        {
            Debug.Assert(logCfg != null);

            var debuggerTarget = new NLog.Targets.OutputDebugStringTarget()
            {
                Name = "debugger",
                Layout = LoggingLayout
            };

            logCfg.AddTarget("debugger", debuggerTarget);

            var rule = new NLog.Config.LoggingRule("*", NLog.LogLevel.Trace, debuggerTarget);
            logCfg.LoggingRules.Add(rule);
        }

        private static void EnsureLoggingPathExist(ShellSettings cfg)
        {
            if (!string.IsNullOrEmpty(cfg.LogPath))
            {
                var logDir = Environment.ExpandEnvironmentVariables(cfg.LogPath);
                if (!Directory.Exists(logDir))
                {
                    Directory.CreateDirectory(logDir);
                }

                var archivesDir = Path.Combine(logDir, LogArchiveDirectoryName);
                if (!Directory.Exists(archivesDir))
                {
                    Directory.CreateDirectory(archivesDir);
                }
            }
        }
    }


}
