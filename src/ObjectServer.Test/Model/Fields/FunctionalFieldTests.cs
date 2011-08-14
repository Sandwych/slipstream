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
        private const string ModelName = "test.functional_field_object";

        private dynamic PrepareTestData()
        {
            dynamic data = new ExpandoObject();

            data.record1 = new ExpandoObject();
            data.record1.name = "test1";
            data.record1.field1 = 1;
            data.record1.field2 = 2;
            data.record1_id = this.Service.CreateModel(this.SessionId, ModelName, data.record1);

            data.record2 = new ExpandoObject();
            data.record2.name = "test2";
            data.record2.field1 = 5;
            data.record2.field2 = 4;
            data.record2_id = this.Service.CreateModel(this.SessionId, ModelName, data.record2);

            return data;
        }

        [SetUp]
        public void ClearTestData()
        {
            var ids = this.Service.SearchModel(this.SessionId, ModelName);
            this.Service.DeleteModel(
                this.SessionId, ModelName, ids.Select(o => (object)o).ToArray());
        }

        [Test]
        public void Test_function_many2one()
        {
            var constraints = new object[][] { new object[] { "login", "=", "root" } };
            var rootId = this.Service.SearchModel(this.SessionId, "core.user", constraints, null, 0, 0)[0];

            dynamic data = PrepareTestData();

            var records = this.Service.ReadModel(this.SessionId, ModelName, new object[] { data.record1_id }, null);

            var userField0 = (object[])records[0]["user"];
            Assert.AreEqual(rootId, userField0[0]);
        }

        [Test]
        public void Test_function_field_as_constraint()
        {
            var constraints = new object[][] { new object[] { "sum_field", "=", 9 } };
            dynamic data = PrepareTestData();

            var n = this.Service.CountModel(this.SessionId, ModelName, constraints);

            Assert.AreEqual(1, n);
        }
    }
}
