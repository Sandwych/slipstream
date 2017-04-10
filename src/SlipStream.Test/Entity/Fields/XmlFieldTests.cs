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
    public class XmlFieldTests : ServiceContextTestCaseBase
    {
        [Test]
        public void CanWriteAndReadXmlField()
        {
            this.ClearTestEntityTable();
            dynamic testModel = this.GetResource("test.test_entity");
            var xml = "<root><tree /></root>";
            var record = new Dictionary<string, object>()
                {
                    { "name", "name1" },
                    { "address", "address1" },
                    { "xml_field", xml },
                };
            var id = (long)testModel.Create(record);

            record = testModel.Read(new object[] { id }, null)[0];

            var field = record["xml_field"] as string;
            Assert.NotNull(field);
            Assert.AreEqual(xml, field);
        }
    }
}
