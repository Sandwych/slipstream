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
            Console.WriteLine("ObjectServer.TestServer 测试用服务器");
            Console.WriteLine("正在初始化 ObjectServer 框架 ...");
            EnsureFrameworkInitialized();
            Console.WriteLine("服务器正在启动 ...");
            var cs = new CoreServer(4);
            var serverThread = new Thread(cs.Start);
            serverThread.Start();

            Console.WriteLine("服务已经启动，按下回车键结束本程序 ...");
            Console.ReadLine();

            serverThread.Abort();

            Console.WriteLine("服务器已经终止");

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
