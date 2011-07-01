using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Configuration;

namespace ObjectServer.TestServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("ObjectServer.TestServer 测试用服务器\n");

            InitializeFramework();

            var rpcThread = StartRpcServer();
            var httpThread = StartHttpServer();

            Console.WriteLine("\n系统启动完毕，开始等待客户端请求。按[回车键]终止本程序");
            Console.ReadLine();

            httpThread.Abort();
            rpcThread.Abort();

            Console.WriteLine("服务器已经终止");

        }

        private static Thread StartRpcServer()
        {
            Console.WriteLine("RPC 服务器正在启动 ...");


            var serverThread = new Thread(() =>
            {
                var cs = new RpcHost();
                cs.Start();
            });
            serverThread.Start();

            Console.WriteLine("RPC 服务已经启动");
            return serverThread;
        }

        private static Thread StartHttpServer()
        {
            Console.WriteLine("HTTP 服务器正在启动 ...");

            var serverThread = new Thread(() =>
            {
                using (var cs = new ObjectServer.Net.HttpServer())
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

            Console.WriteLine("日志文件目录=[{0}]，RPC 服务主机 URL=[{1}]",
                Infrastructure.Configuration.LogPath,
                Infrastructure.Configuration.RpcHostUrl);
        }

        private static void EnsureFrameworkInitialized()
        {
            //TODO: 初始化为测试配置
            if (!Infrastructure.Initialized)
            {
                var cfg = (Config)ConfigurationManager.GetSection("objectserver-config");
                Infrastructure.Initialize(cfg);
            }
        }
    }
}
