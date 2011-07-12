using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace ObjectServer
{
    public sealed class RpcHost
    {
        public RpcHost()
        {

            if (!Platform.Initialized)
            {
                throw new InvalidOperationException("无法应用启动服务器，请先初始化框架");
            }

            if (Platform.Configuration.RpcHandlerMax <= 0)
            {
                throw new IndexOutOfRangeException("无效的工人数量");
            }

            this.RpcHandlerMax = Platform.Configuration.RpcHandlerMax;
            this.RpcHandlerUrl = Platform.Configuration.RpcHandlerUrl;
            this.RpcHostUrl = Platform.Configuration.RpcHostUrl;
        }

        public int RpcHandlerMax { get; private set; }
        public string RpcHandlerUrl { get; private set; }
        public string RpcHostUrl { get; private set; }

        public void Start()
        {
            var workersUrl = this.RpcHandlerUrl;
            var hostUrl = this.RpcHostUrl;

            Logger.Info(() => string.Format(
                "正在启动应用服务器 RPC 处理器线程：远程调用主机 URL=[{0}]，RPC 处理器线程 URL=[{1}]",
                hostUrl, workersUrl));

            //TODO 写个自己的 WorkerPool，允许跨进程，并且要受 Supervisor 的控制，能够体面终止
            var pool =
                new ZMQ.ZMQDevice.WorkerPool(
                    hostUrl, workersUrl, RpcHandler.Start, (short)this.RpcHandlerMax);

            Thread.Sleep(Timeout.Infinite);
        }
    }
}
