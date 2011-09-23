using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using Kayak;
using Kayak.Http;

namespace ObjectServer.Http
{
    internal class SchedulerDelegate : ISchedulerDelegate
    {
        private KayakHttpServer httpServer;

        public SchedulerDelegate(KayakHttpServer hs)
        {
            if (hs == null)
            {
                throw new ArgumentNullException("hs");
            }

            this.httpServer = hs;
        }

        public void OnException(IScheduler scheduler, Exception e)
        {
            // TODO 完善错误处理
            // 这里容易出错的地方应该就是 ZMQ 了
            LoggerProvider.EnvironmentLogger.Error("Error on scheduler.", e);
            throw e;
        }

        public void OnStop(IScheduler scheduler)
        {
            if (scheduler == null)
            {
                throw new ArgumentNullException("scheduler");
            }

            //这里我们假设调用了 IScheduler.Stop() 以后 Kayak Scheduler 总是能成功停止
            //TODO 以后也许需要在这里通知 HttpServer Scheduler 已经成功停止

        }
    }
}
