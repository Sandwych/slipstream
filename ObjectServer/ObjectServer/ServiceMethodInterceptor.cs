using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Transactions;

using Castle.DynamicProxy;

namespace ObjectServer
{
    internal sealed class ServiceMethodInterceptor : StandardInterceptor
    {
        protected override void PerformProceed(IInvocation invocation)
        {
            try
            {
                base.PerformProceed(invocation);
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                throw ex;
            }
        }
    }
}
