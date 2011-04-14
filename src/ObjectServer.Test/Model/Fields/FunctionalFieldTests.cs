using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;

using ObjectServer.Model;

namespace ObjectServer.Model.Test
{
    [TestFixture]
    public class FunctionalFieldTests : LocalTestCase
    {
        [Test]
        public void Test_function_many2one()
        {
            var rootDomain = new object[][] { new object[] { "login", "=", "root" } };
            var rootId = this.Service.SearchModel(this.SessionId, "core.user", rootDomain, null, 0, 0)[0];
            var record = new Dictionary<string, object>()
            {
                { "name", "test1" },
            };

            var id = this.Service.CreateModel(
                this.SessionId, "test.functional_field_object", record);
            var records = this.Service.ReadModel(
                this.SessionId, "test.functional_field_object", new object[] { id }, null);

            var userField0 = (object[])records[0]["user"];
            Assert.AreEqual(rootId, userField0[0]);

        }
    }
}
