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
            long[] ids = (long[])this.Service.Execute(this.SessionId, "core.module", "Search", null, null, 0, 0);

            var threadProc = new ThreadStart(() =>
                {
                    Assert.DoesNotThrow(() =>
                    {
                        var sid = this.Service.LogOn("objectserver", "root", "root");

                        //每个线程中读取10次
                        const int ReadTimes = 10;
                        for (int i = 0; i < ReadTimes; i++)
                        {
                            this.Service.Execute(sid, "core.module", "Read", ids, null);
                        }

                        this.Service.LogOff(sid);
                    });
                });

            //启动十个线程并发测试
            var n = 10;
            var threads = new List<Thread>();
            for (int i = 0; i < n; i++)
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
