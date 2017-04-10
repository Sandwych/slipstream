using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;

using NUnit.Framework;

using SlipStream.Entity;

namespace SlipStream.Entity.Fields
{
    [TestFixture(Category = "ORM")]
    public class ScalarFieldTests : ServiceContextTestCaseBase
    {
        [Test]
        public void CanWriteAndReadDateTimeField()
        {
            this.ClearTestEntityTable();
            dynamic testModel = this.GetResource("test.test_entity");
            var now = DateTime.Now;
            var dateField = now.Date;
            var timeField = now;
            var datetimeField = now;
            var record = new Dictionary<string, object>()
                {
                    { "name", "name1" },
                    { "address", "address1" },
                    { "date_field", dateField },
                    { "time_field", timeField },
                    { "datetime_field", datetimeField },
                };
            var id = (long)testModel.Create(record);

            record = testModel.Read(new object[] { id }, null)[0];

            var dateField1 = (DateTime)record["date_field"];
            Assert.AreEqual(now.DayOfYear, dateField1.DayOfYear);
        }
    }
}
