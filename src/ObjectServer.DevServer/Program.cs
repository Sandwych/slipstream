using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Configuration;
using System.Diagnostics;

using Mono.Options;

namespace ObjectServer.Server
{
    class DevServerProgram
    {
        private static ShellSettings CreateSettingsFromProgramArgs(string[] args)
        {
            bool isShowHelp = false;
            bool isShowVersion = false;
            string configPath = null;
            var p = new OptionSet() {
                { "h|?|help",  v => isShowHelp = v != null },
                { "version",   v => isShowVersion = v != null },
                { "c|_shellSettings=", v => configPath = v },
            };

            var extra = p.Parse(args);

            if (isShowHelp)
            {
                Console.WriteLine("Usage:");
                Console.WriteLine("  slipserver.exe [Options]");
                Console.WriteLine();
                Console.WriteLine("Options:");
                Console.WriteLine("  -c CONF_FILE\t\tLoad settings from CONF_FILE");
                Console.WriteLine("  --version\t\tShow version information");
                Console.WriteLine("  -h|-?|--help\t\tShow this help text");
                System.Environment.Exit(0);
            }

            if (isShowVersion)
            {
                //TODO 显示版本
                System.Environment.Exit(0);
            }

            if (string.IsNullOrEmpty(configPath))
            {
                return new ShellSettings();
            }
            else
            {
                return ShellSettings.Load(configPath);
            }
        }

        private static void OnExit(object sender, EventArgs args)
        {
            Console.WriteLine("ObjectServer Server was existed successfully.");
        }

        static int Main(string[] args)
        {
            Console.WriteLine("ObjectServer Development Server\n");

            try
            {
                InitializeFramework(args);
                AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnExit);

                using (var server = new ServerProcess())
                {
                    var serverThreadProc = new ThreadStart(delegate
                    {
                        server.Run();
                    });

                    var serverThread = new Thread(serverThreadProc);
                    serverThread.Start();

                    Console.WriteLine("开始等待客户端请求...");
                    WaitToQuit();

                    Console.WriteLine("开始广播停止命令...");
                    var role = SlipstreamEnvironment.Settings.Role;
                    if (role == ServerRoles.Standalone || role == ServerRoles.Controller)
                    {
                        server.BeginStopAll();
                    }
                    else if (role == ServerRoles.HttpServer)
                    {
                        server.BeginStopHttpServer();
                    }
                    else if (role == ServerRoles.Worker)
                    {
                        server.BeginStopRpcWorkers();
                    }

                    Console.WriteLine("服务器终止进程开始...");
                }

                return 0;
            }
            catch (OptionException ex)
            {
                Console.WriteLine("Bad argument(s): ");
                Console.WriteLine(ex.Message);
                Console.WriteLine("Try `osdevsvr.exe --help' for more information.");
                return -1;
            }
            catch (Exception ex)
            {
                LoggerProvider.EnvironmentLogger.ErrorException(
                    "Uncached Exception: " + ex.Message, ex);
                var oldColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("未捕获的服务器异常：[{0}]", ex.Message);
                Console.ForegroundColor = oldColor;
                return -1;
            }
            finally
            {
                SlipstreamEnvironment.Shutdown();
            }
        }

        private static void WaitToQuit()
        {
            do
            {
                Console.WriteLine("按 'Q' 键终止服务");
            } while (Char.ToUpperInvariant(Console.ReadKey(true).KeyChar) != 'Q');
        }

        private static void InitializeFramework(string[] args)
        {

            Debug.Assert(!SlipstreamEnvironment.Initialized);

            var cfg = CreateSettingsFromProgramArgs(args);
            cfg.LogToConsole = true;

            Console.WriteLine("正在初始化 ObjectServer 框架 ...");
            SlipstreamEnvironment.Initialize(cfg);

            Console.WriteLine("ObjectServer 框架已经成功初始化");

            Console.WriteLine("日志文件目录=[{0}]，应用服务器主机 URL=[{1}]",
                SlipstreamEnvironment.Settings.LogPath,
                SlipstreamEnvironment.Settings.RpcBusUrl);
        }

    }
}
