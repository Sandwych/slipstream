using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Castle.DynamicProxy;

namespace ObjectServer
{
    internal sealed class MethodCachingInterceptor : StandardInterceptor
    {
        protected override void PerformProceed(IInvocation invocation)
        {
            //TODO 实现方法缓存 Caching

            base.PerformProceed(invocation);
        }
    }
}
