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
                var server = new ServerProcess();
                server.Run();
                return 0;
            }
            catch (Exception ex)
            {
                var oldColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("启动服务器失败，异常信息：[{0}]", ex.Message);
                Console.ForegroundColor = oldColor;
                return -1;
            }
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
