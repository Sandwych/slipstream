using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;

using ObjectServer.Model;

namespace ObjectServer.Model.Test
{
    [TestFixture]
    public class SearchTests : LocalTestCase
    {
        [Test]
        public void Test_search_limit()
        {
            var domain = new object[][] { new object[] { "name", "like", "%" } };
            var ids = this.Service.SearchModel(this.SessionId, "core.model",
                domain, null, 0, 2);
            Assert.AreEqual(2, ids.Length);

            ids = this.Service.SearchModel(this.SessionId, "core.model",
                domain, null, 0, 3);
            Assert.AreEqual(3, ids.Length);
        }

        [Test]
        public void Test_search_offset()
        {
            var domain = new object[][] { new object[] { "name", "like", "%" } };
            var ids1 = this.Service.SearchModel(this.SessionId, "core.model",
                domain, null, 0, 2);
            var ids2 = this.Service.SearchModel(this.SessionId, "core.model",
                domain, null, 1, 2);
            Assert.AreNotEqual(ids1[0], ids2[0]);
            Assert.AreEqual(ids1[1], ids2[0]);

        }

        [Test]
        public void Test_domain_equal_operator()
        {
            var domain = new object[][] { 
                new object[] {  "name", "=", "core.model" } 
            };

            var ids = this.Service.SearchModel(this.SessionId, "core.model", domain);
            Assert.AreEqual(1, ids.Length);

            domain = new object[][] {
                new object[] { "name", "=", "a dummy model" }
            };
            ids = this.Service.SearchModel(this.SessionId, "core.model", domain);
            Assert.AreEqual(0, ids.Length);
        }

        [Test]
        public void Test_domain_like_operator()
        {
            var domain = new object[][] { new object[] { "name", "like", "core.modu%" } };
            var ids = this.Service.SearchModel(this.SessionId, "core.model", domain);
            Assert.AreEqual(1, ids.Length);

            domain = new object[][] { new object[] { "name", "like", "%like dummy%" } };
            ids = this.Service.SearchModel(this.SessionId, "core.model", domain);
            Assert.AreEqual(0, ids.Length);

            domain = new object[][] { new object[] { "name", "like", "core.modul_" } };
            ids = this.Service.SearchModel(this.SessionId, "core.model", domain);
            Assert.AreEqual(1, ids.Length);
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
            var ids = this.Service.SearchModel(this.SessionId, "core.model", domain, null, 0, 0);
            Assert.AreEqual(3, ids.Length);
        }

        [Test]
        public void Test_domain_notin_operator()
        {
            var allIds = this.Service.SearchModel(this.SessionId, "core.model", null, null, 0, 0);

            var notinDomain = new object[][] { 
                new object[] { 
                    "name", "!in", 
                    new object[] { "core.model", "core.field" } 
                } 
            };
            var ids = this.Service.SearchModel(this.SessionId, "core.model", notinDomain, null, 0, 0);

            Assert.AreEqual(allIds.Length, ids.Length + 2);
        }

        [Test]
        public void Test_simple_searching_order()
        {
            var ascOrder = new object[][] { new object[] { "id", "asc" } };
            var descOrder = new object[][] { new object[] { "id", "desc" } };

            var ascIds = this.Service.SearchModel(this.SessionId, "core.model", null, ascOrder, 0, 0);
            var descIds = this.Service.SearchModel(this.SessionId, "core.model", null, descOrder, 0, 0);

            Assert.AreEqual(ascIds.Length, descIds.Length);
            Assert.AreNotEqual(ascIds[0], descIds[0]);
            Assert.AreEqual(ascIds.Last(), descIds.First());
        }

    }
}
