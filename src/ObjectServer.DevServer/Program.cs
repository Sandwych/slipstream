using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Configuration;

namespace ObjectServer.Daemon
{
    class DevServerProgram
    {
        static int Main(string[] args)
        {
            Console.WriteLine("ObjectServer 开发服务器\n");

            RpcHostWorker rpcThread;
            Thread httpThread;
            var controller = new Messaging.WorkerCommander();

            try
            {
                InitializeFramework();

                controller = new Messaging.WorkerCommander();
                controller.Start();

                rpcThread = StartApplicationServer();
                httpThread = StartHttpServer();
            }
            catch (Exception ex)
            {
                var oldColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("启动服务器失败，异常信息：[{0}]", ex.Message);
                Console.ForegroundColor = oldColor;
                return -1;
            }

            Console.WriteLine("\n系统启动完毕，开始等待客户端请求...");
            do
            {
                Console.WriteLine("按 'Q' 键终止服务");
            } while (Char.ToUpperInvariant(Console.ReadKey(true).KeyChar) != 'Q');

            Console.WriteLine("开始广播停止命令...");
            controller.Broadcast("STOP");

            controller.Dispose();
            Console.WriteLine("服务器已经终止");

            return 0;

        }

        private static RpcHostWorker StartApplicationServer()
        {
            Console.WriteLine("应用服务器正在启动 ...");

            var rpcHostWorker = new RpcHostWorker();
            rpcHostWorker.Start();

            Console.WriteLine("应用服务已经启动");
            return rpcHostWorker;
        }

        private static Thread StartHttpServer()
        {
            Console.WriteLine("HTTP 服务器正在启动 ...");

            var serverThread = new Thread(() =>
            {
                using (var cs = new ObjectServer.Http.HttpServer(
                    Environment.Configuration.CommanderUrl,
                    Environment.Configuration.RpcHostUrl,
                    Environment.Configuration.HttpListenPort))
                {
                    cs.Start();

                }
            });
            serverThread.Start();
            Console.WriteLine("HTTP 服务已经启动");
            return serverThread;
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
