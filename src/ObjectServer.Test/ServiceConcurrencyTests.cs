using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;
using System.Threading;

using NUnit.Framework;

using ObjectServer.Sql;
using ObjectServer.Model;

namespace ObjectServer.Test
{
    [TestFixture]
    public sealed class ServiceConcurrencyTests : TransactionTestCaseBase
    {
        [Test]
        public void TestMultithreadRead()
        {

            var threadProc = new ThreadStart(this.TestProc);

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

        private void TestProc()
        {
            var service = Environment.ExportedService;
            var ids = (long[])service.Execute(TestingDatabaseName,
                base.SessionId, "core.menu", "Search",
                null, null, 0, 0);

            //每个线程中读取5次
            const int ReadTimes = 5;
            for (int i = 0; i < ReadTimes; i++)
            {
                dynamic records = service.Execute(TestingDatabaseName, base.SessionId, "core.menu", "Read", ids, null);
                Assert.AreEqual(ids.Length, records.Length);
            }
        }


    }
}
