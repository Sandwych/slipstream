using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;

using ObjectServer.Model;

namespace ObjectServer.Model.Test
{
    [TestFixture]
    public class SearchTests : LocalTestBase
    {
        IService proxy = new ServiceDispatcher();

        [Test]
        public void Test_search_limit()
        {
            var ids = proxy.SearchModel(this.SessionId, "core.model",
                new object[][] { new object[] {"name", "like", "%" } }, 0, 2);
            Assert.AreEqual(2, ids.Length);

            ids = proxy.SearchModel(this.SessionId, "core.model",
                new object[][] { new object[] {"name", "like", "%" } }, 0, 3);
            Assert.AreEqual(3, ids.Length);
        }
    }
}
