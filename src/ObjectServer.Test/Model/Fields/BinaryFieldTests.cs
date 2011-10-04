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
    public class BinaryFieldTests : LocalTestCase
    {
        [Test]
        public void test_write_and_read_binary_field()
        {
            this.ClearTestModelTable();
            dynamic testModel = this.TransactionContext.GetResource("test.test_model");
            var fieldData = new byte[] { 33, 44, 55, 66, 77 };
            var record = new Dictionary<string, object>()
                {
                    { "name", "name1" },
                    { "address", "address1" },
                    { "binary_field", fieldData },
                };
            var id = (long)testModel.Create(this.TransactionContext, record);

            record = testModel.Read(this.TransactionContext, new object[] { id }, null)[0];

            var field = record["binary_field"] as byte[];
            Assert.NotNull(field);
            Assert.AreEqual(5, field.Length);
            Assert.AreEqual(fieldData[0], field[0]);
            Assert.AreEqual(fieldData[4], field[4]);
        }

        [Test]
        public void test_write_search_and_count_binary_field()
        {
            this.ClearTestModelTable();
            dynamic testModel = this.TransactionContext.GetResource("test.test_model");
            var fieldData1 = new byte[] { 33, 44, 55, 66, 77 };
            var fieldData2 = new byte[] { 44, 44, 55, 66, 77 };

            dynamic record1 = new ExpandoObject();
            record1.name = "name1";
            record1.address = "address1";
            record1.binary_field = fieldData1;
            var id1 = (long)testModel.Create(this.TransactionContext, record1);

            dynamic record2 = new ExpandoObject();
            record2.name = "name2";
            record2.address = "address2";
            record2.binary_field = fieldData2;
            var id2 = (long)testModel.Create(this.TransactionContext, record2);

            var constraints = new object[][] {
                new object[] { "binary_field", "=", fieldData1 },
            };

            var n = this.Service.CountModel(TestingDatabaseName, this.SessionId, "test.test_model", constraints);
            Assert.AreEqual(1, n);

            var ids = this.Service.SearchModel(TestingDatabaseName, this.SessionId, "test.test_model", constraints);
            Assert.AreEqual(1, ids.Length);
            Assert.AreEqual(id1, ids[0]);
        }
    }
}
