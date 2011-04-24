using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;

namespace ObjectServer.Test
{
    [TestFixture]
    public class ServiceDispatcherTests : LocalTestCase
    {

        [Test]
        public void Test_search_model()
        {
            long[] ids = new long[] { };
            Assert.DoesNotThrow(() =>
            {
                ids = this.Service.SearchModel(this.SessionId, "core.model");
            });

            Assert.That(ids.Length > 0);
        }

    }
}
