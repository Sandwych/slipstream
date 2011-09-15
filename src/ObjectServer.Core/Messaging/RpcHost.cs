using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;

namespace ObjectServer
{
    public sealed class RpcHost
    {
        public RpcHost()
        {

            if (!Environment.Initialized)
            {
                throw new InvalidOperationException("无法应用启动服务器，请先初始化框架");
            }

            if (Environment.Configuration.RpcHandlerMax <= 0)
            {
                throw new IndexOutOfRangeException("无效的工人数量");
            }

            this.RpcHandlerMax = Environment.Configuration.RpcHandlerMax;
            this.RpcHandlerUrl = Environment.Configuration.RpcHandlerUrl;
            this.RpcHostUrl = Environment.Configuration.RpcHostUrl;
        }

        public int RpcHandlerMax { get; private set; }
        public string RpcHandlerUrl { get; private set; }
        public string RpcHostUrl { get; private set; }

        public void Start()
        {
            var workersUrl = this.RpcHandlerUrl;
            var hostUrl = this.RpcHostUrl;

            LoggerProvider.EnvironmentLogger.Info(() => string.Format(
                "Starting all RPC-Handler threads: RPC-Entrance URL=[{0}]，RPC-Hander URL=[{1}]",
                hostUrl, workersUrl));

            //TODO 写个自己的 WorkerPool，允许跨进程，并且要受 Supervisor 的控制，能够体面终止
            using (var pool = new ZMQ.ZMQDevice.WorkerPool(hostUrl, workersUrl, RpcHandler.ProcessingLoop, (short)this.RpcHandlerMax))
            {
                Wait(pool);
            }
        }

        private static void Wait(ZMQ.ZMQDevice.WorkerPool pool)
        {
            using (var subSocket = new ZMQ.Socket(ZMQ.SocketType.SUB))
            {
                var subAddress = Environment.Configuration.ControllerUrl;
                subSocket.Connect(subAddress);
                subSocket.Subscribe("STOP", Encoding.UTF8);
                while (true)
                {
                    var cmd = subSocket.Recv(Encoding.UTF8);

                    if (cmd == "STOP" && pool.IsRunning)
                    {
                        LoggerProvider.RpcLogger.Info("'STOP' command received, try to stop the WorkerPool...");
                        pool.Stop();
                        while (pool.IsRunning)
                        {
                            Thread.Sleep(100);
                        }
                        Debug.Assert(!pool.IsRunning);
                        break;
                    }
                }

                LoggerProvider.RpcLogger.Info("RPC Host is stopped.");
            }
        }
    }
}
