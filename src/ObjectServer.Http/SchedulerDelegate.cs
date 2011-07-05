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
            Logger.Error("Error on scheduler.", e);
        }

        public void OnStop(IScheduler scheduler)
        {

        }
    }
}
