using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;

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
            dynamic record = new ExpandoObject();
            record.name = "test1";
            record.field1 = 1;
            record.field2 = 2;

            var id = this.Service.CreateModel(
                this.SessionId, "test.functional_field_object", record);
            var records = this.Service.ReadModel(
                this.SessionId, "test.functional_field_object", new object[] { id }, null);

            var userField0 = (object[])records[0]["user"];
            Assert.AreEqual(rootId, userField0[0]);

        }
    }
}
