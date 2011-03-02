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
        IExportedService proxy = ObjectServerStarter.ExportedService;

        [Test]
        public void Test_search_limit()
        {
            var domain = new object[][] { new object[] { "name", "like", "%" } };
            var ids = proxy.SearchModel(this.SessionId, "core.model",
                domain, 0, 2);
            Assert.AreEqual(2, ids.Length);

            ids = proxy.SearchModel(this.SessionId, "core.model",
                domain, 0, 3);
            Assert.AreEqual(3, ids.Length);
        }

        [Test]
        public void Test_search_offset()
        {
            var domain = new object[][] { new object[] { "name", "like", "%" } };
            var ids1 = proxy.SearchModel(this.SessionId, "core.model",
                domain, 0, 2);
            var ids2 = proxy.SearchModel(this.SessionId, "core.model",
                domain, 1, 2);
            Assert.AreNotEqual(ids1[0], ids2[0]);
            Assert.AreEqual(ids1[1], ids2[0]);

        }

        [Test]
        public void Test_domain_in_operator()
        {
            var domain = new object[][] { 
                new object[] { 
                    "name", "in", 
                    new object[] { "core.model", "core.field", "core.module" } 
                } 
            };
            var ids = proxy.SearchModel(this.SessionId, "core.model", domain, 0, 0);
            Assert.AreEqual(3, ids.Length);
        }

    }
}
