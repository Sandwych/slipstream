using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;

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

        [Test]
        public void Test_CRUD_model()
        {
            base.ClearTestModelTable();
            dynamic testRecord = new ExpandoObject();
            testRecord.name = "name1";
            testRecord.address = "address1";

            long testId1 = 0;
            Assert.DoesNotThrow(() =>
            {
                testId1 = this.Service.CreateModel(this.SessionId, "test.test_model", testRecord);
            });

            Assert.That(testId1 > 0);

            var ids = new long[] { };
            Assert.DoesNotThrow(() =>
            {
                var constraints = new object[][] { new object[] { "name", "=", "name1" } };
                ids = this.Service.SearchModel(this.SessionId, "test.test_model", constraints);
            });
            Assert.AreEqual(1, ids.Length);
            Assert.AreEqual(testId1, ids[0]);

            dynamic newFieldValues = new ExpandoObject();
            newFieldValues.name = "name2";
            Assert.DoesNotThrow(() =>
            {
                this.Service.WriteModel(this.SessionId, "test.test_model", testId1, newFieldValues);
            });

            IDictionary<string, object>[] records = null;
            var oIds = ids.Select(id => (object)id).ToArray();
            Assert.DoesNotThrow(() =>
            {
                records = this.Service.ReadModel(this.SessionId, "test.test_model", oIds);
            });

            Assert.AreEqual(1, records.Length);
            var record = records[0];
            Assert.AreEqual("name2", record["name"]);

            Assert.DoesNotThrow(() =>
            {
                this.Service.DeleteModel(this.SessionId, "test.test_model", oIds);
            });

            ids = this.Service.SearchModel(this.SessionId, "test.test_model");
            Assert.AreEqual(0, ids.Length);

        }

        [Test]
        public void Test_execute_user_service(
            [Random(-9999, 9999, 1)] int x, [Random(-9999, 9999, 1)] int y)
        {
            var result = this.Service.Execute(
                this.SessionId, "test.test_model", "GetNumberPlusResult", x, y);
            Assert.IsInstanceOf<int>(result);
            Assert.AreEqual(x + y, (int)result);
        }

    }
}
