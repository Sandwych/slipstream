using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;
using System.Threading;

using NUnit.Framework;

using ObjectServer.Sql;
using ObjectServer.Model;

namespace ObjectServer.Model.Test
{
    [TestFixture]
    public sealed class ConcurrencyTests : LocalTestCase
    {
        [Test]
        public void TestMultithreadRead()
        {
            long[] ids = (long[])this.Service.Execute(this.SessionId, "core.menu", "Search", null, null, 0, 0);

            var threadProc = new ThreadStart(() =>
                {
                    Assert.DoesNotThrow(() =>
                    {
                        //每个线程中读取5次
                        const int ReadTimes = 5;
                        for (int i = 0; i < ReadTimes; i++)
                        {
                            this.Service.Execute(this.SessionId, "core.menu", "Read", ids, null);
                        }
                    });
                });

            //启动多个线程并发测试
            const int ThreadCount = 50;
            var threads = new List<Thread>();
            for (int i = 0; i < ThreadCount; i++)
            {
                var t = new Thread(threadProc);
                threads.Add(t);
                t.Start();
            }

            //等待全部线程结束
            foreach (var t in threads)
            {
                t.Join();
            }
        }


    }
}
