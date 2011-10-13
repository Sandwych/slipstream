using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Configuration;

namespace ObjectServer.Server
{
    class DevServerProgram
    {
        private static void OnExit(object sender, EventArgs args)
        {
            Console.WriteLine("ObjectServer 开发服务器进程已经终止.");
        }

        static int Main(string[] args)
        {
            Console.WriteLine("ObjectServer 开发服务器\n");

            InitializeFramework();

            AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnExit);

            try
            {
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
                    server.BeginStop();
                    Console.WriteLine("服务器终止进程启动...");
                }

                return 0;
            }
            catch (Exception ex)
            {
                LoggerProvider.EnvironmentLogger.Error(
                    "Uncached Exception: " + ex.Message, ex);
                var oldColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("未捕获的服务器异常：[{0}]", ex.Message);
                Console.ForegroundColor = oldColor;
                return -1;
            }
            finally
            {
                Environment.Shutdown();
            }
        }

        private static void WaitToQuit()
        {
            do
            {
                Console.WriteLine("按 'Q' 键终止服务");
            } while (Char.ToUpperInvariant(Console.ReadKey(true).KeyChar) != 'Q');
        }

        private static void InitializeFramework()
        {
            Console.WriteLine("正在初始化 ObjectServer 框架 ...");
            EnsureFrameworkInitialized();
            Console.WriteLine("ObjectServer 框架已经成功初始化");

            Console.WriteLine("日志文件目录=[{0}]，应用服务器主机 URL=[{1}]",
                Environment.Configuration.LogPath,
                Environment.Configuration.RpcHostUrl);
        }

        private static void EnsureFrameworkInitialized()
        {
            if (!Environment.Initialized)
            {
                var cfg = new Config();
                cfg.LogToConsole = true;

                Environment.Initialize(cfg);
            }
        }
    }
}
