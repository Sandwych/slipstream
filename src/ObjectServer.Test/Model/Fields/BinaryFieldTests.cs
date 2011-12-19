using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;

using NUnit.Framework;

using ObjectServer.Model;

namespace ObjectServer.Model.Fields
{
    [TestFixture]
    public class BinaryFieldTests : ServiceContextTestCaseBase
    {
        [Test]
        public void CanWriteAndReadBinaryField()
        {
            this.ClearTestModelTable();
            dynamic testModel = this.GetResource("test.test_model");
            var fieldData = new byte[] { 33, 44, 55, 66, 77 };
            var record = new Dictionary<string, object>()
                {
                    { "name", "name1" },
                    { "address", "address1" },
                    { "binary_field", fieldData },
                };
            var id = (long)testModel.Create(record);

            record = testModel.Read(new object[] { id }, null)[0];

            var field = record["binary_field"] as byte[];
            Assert.NotNull(field);
            Assert.AreEqual(5, field.Length);
            Assert.AreEqual(fieldData[0], field[0]);
            Assert.AreEqual(fieldData[4], field[4]);
        }

        [Test]
        public void CanSearchAndCountBinaryField()
        {
            this.ClearTestModelTable();
            dynamic testModel = this.GetResource("test.test_model");
            var fieldData1 = new byte[] { 33, 44, 55, 66, 77 };
            var fieldData2 = new byte[] { 44, 44, 55, 66, 77 };

            dynamic record1 = new ExpandoObject();
            record1.name = "name1";
            record1.address = "address1";
            record1.binary_field = fieldData1;
            var id1 = (long)testModel.Create(record1);

            dynamic record2 = new ExpandoObject();
            record2.name = "name2";
            record2.address = "address2";
            record2.binary_field = fieldData2;
            var id2 = (long)testModel.Create(record2);

            var constraints = new object[][] {
                new object[] { "binary_field", "=", fieldData1 },
            };

            var n = testModel.Count(constraints);
            Assert.AreEqual(1, n);

            var ids = testModel.Search(constraints, null, 0, 0);
            Assert.AreEqual(1, ids.Length);
            Assert.AreEqual(id1, ids[0]);
        }
    }
}
