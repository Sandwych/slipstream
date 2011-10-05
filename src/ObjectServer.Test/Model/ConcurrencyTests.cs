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
    public sealed class ConcurrencyTests : UserLoggedTestCaseBase
    {
        [Test]
        public void TestMultithreadRead()
        {
            var menuModel = this.GetResource("core.menu");
            var ids = (long[])menuModel.Search(this.TransactionContext, null, null, 0, 0);

            var threadProc = new ThreadStart(() =>
            {
                //每个线程中读取5次
                const int ReadTimes = 5;
                for (int i = 0; i < ReadTimes; i++)
                {
                    menuModel.Read(this.TransactionContext, ids, null);
                }
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
