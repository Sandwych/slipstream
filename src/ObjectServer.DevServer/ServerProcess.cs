using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using ObjectServer;

namespace ObjectServer.Server
{
    public sealed class ServerProcess
    {
        public void Run()
        {
            using (var supervisor = new Server.Supervisor())
            {
                supervisor.Start();

                var cfg = Environment.Configuration;
                if(cfg.Role == ServerRoles.Standalone || cfg.Role == ServerRoles.Worker)
                {
                    var rpcHostWorker = StartApplicationServer();
                }

                if(cfg.Role == ServerRoles.Standalone || cfg.Role == ServerRoles.HttpServer)
                {
                    var httpThread = StartHttpServer();
                }


                Console.WriteLine("\n系统启动完毕，开始等待客户端请求...");
                do
                {
                    Console.WriteLine("按 'Q' 键终止服务");
                } while (Char.ToUpperInvariant(Console.ReadKey(true).KeyChar) != 'Q');

                Console.WriteLine("开始广播停止命令...");
                supervisor.StopAll();

                Console.WriteLine("服务器已经终止");
            }
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
                using (var cs = new ObjectServer.Http.AnnaHttpServer(
                    Environment.Configuration.BroadcastUrl,
                    Environment.Configuration.RpcHostUrl,
                    Environment.Configuration.HttpListenUrl))
                {
                    cs.Start();

                }
            });
            serverThread.Start();
            Console.WriteLine("HTTP 服务已经启动");
            return serverThread;
        }

    }
}
