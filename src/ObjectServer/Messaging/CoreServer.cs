using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace ObjectServer
{
    public class CoreServer
    {
        public CoreServer(short maxThreads)
        {
            if (maxThreads <= 0)
            {
                throw new ArgumentOutOfRangeException("maxThreads");
            }

            this.MaxThreads = maxThreads;
        }

        public short MaxThreads { get; private set; }

        public void Start()
        {
            var workersUrl = "inproc://workers";
            var serviceUrl = "tcp://*:5555";

            Logger.Info(() => string.Format(
                "正在启动核心服务器线程：服务侦听 URL=[{0}]，业务处理线程 URL=[{1}]",
                serviceUrl, workersUrl));

            if (!Infrastructure.Initialized)
            {
                throw new InvalidOperationException("无法启动服务器，请先初始化框架");
            }

            var pool =
                new ZMQ.ZMQDevice.WorkerPool(serviceUrl, workersUrl, RpcHandler.Start, this.MaxThreads);

            Thread.Sleep(Timeout.Infinite);
        }
    }
}
