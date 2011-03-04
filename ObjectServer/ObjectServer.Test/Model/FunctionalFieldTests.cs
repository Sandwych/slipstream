using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;

using ObjectServer.Model;

namespace ObjectServer.Model.Test
{
    [Ignore]
    [TestFixture]
    public class FunctionalFieldTests : LocalTestBase
    {
        [Test]
        public void Test_function_many2one()
        {
            var rootId = (long)1;
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
