using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;

using ObjectServer.Model;

namespace ObjectServer.Model.Test
{
    [TestFixture]
    public class InheritTests : LocalTestCase
    {
        [Test]
        public void Test_single_table()
        {
            dynamic inheritedModel = this.ResourceScope.DatabaseProfile.GetResource("test.single_table");
            Assert.True(inheritedModel.Fields.ContainsKey("age"));

            var propBag = new Dictionary<string, object>()
                {
                    { "name", "inherited" },
                    { "age", 44},
                };

            object id = inheritedModel.Create(this.ResourceScope, propBag);

            var record = inheritedModel.Read(this.ResourceScope, new object[] { id }, null)[0];

            Assert.AreEqual(33, record["age"]);
        }

    }
}
