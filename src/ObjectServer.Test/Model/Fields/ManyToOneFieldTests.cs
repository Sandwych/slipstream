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
    public class ManyToOneFieldTests : LocalTestCase
    {

        [Test]
        public void Test_read_nullable_many_to_one_field()
        {
            this.ClearMasterAndChildTable();

            var nameFieldValue = "child_with_empty_master_field";
            var child = new Dictionary<string, object>()
            {
                { "name", nameFieldValue },
                { "master", null },
            };

            var childModel = (IModel)this.ServiceScope.GetResource("test.child");

            var id = childModel.CreateInternal(this.ServiceScope, child);

            var children = childModel.ReadInternal(
                this.ServiceScope, new long[] { id }, new string[] { "name", "master" });
            var record = children[0];

            Assert.True(record["master"].IsNull());
            Assert.AreEqual(nameFieldValue, (string)record["name"]);
        }
    }
}
