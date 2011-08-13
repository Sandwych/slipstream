using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;


using Kayak;
using Kayak.Http;

namespace ObjectServer.Http
{
    class SchedulerDelegate : ISchedulerDelegate
    {
        public void OnException(IScheduler scheduler, Exception e)
        {
            // TODO 完善错误处理
            // 这里容易出错的地方应该就是 ZMQ 了
            LoggerProvider.BizLogger.Error("Error on scheduler.", e);
        }

        public void OnStop(IScheduler scheduler)
        {
            if (scheduler == null)
            {
                throw new ArgumentNullException("scheduler");
            }

        }
    }
}
