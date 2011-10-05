using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;

using NUnit.Framework;

using ObjectServer.Model;

namespace ObjectServer.Model.Fields.Test
{
    [TestFixture]
    public class ManyToOneFieldTests : TransactionContextTestCaseBase
    {

        [Test]
        public void CanReadNullableManyToOneField()
        {
            this.ClearMasterAndChildTable();

            var nameFieldValue = "child_with_empty_master_field";
            var child = new Dictionary<string, object>()
            {
                { "name", nameFieldValue },
                { "master", null },
            };

            var childModel = (IModel)this.TransactionContext.GetResource("test.child");

            var id = childModel.CreateInternal(this.TransactionContext, child);

            var children = childModel.ReadInternal(
                this.TransactionContext, new long[] { id }, new string[] { "name", "master" });
            var record = children[0];

            Assert.True(record["master"].IsNull());
            Assert.AreEqual(nameFieldValue, (string)record["name"]);
        }

        [Test]
        public void CanWriteNullableManyToOneField()
        {
            this.ClearMasterAndChildTable();

            var nameFieldValue = "child_with_empty_master_field";
            var child = new Dictionary<string, object>()
            {
                { "name", nameFieldValue },
                { "master", null },
            };

            var childModel = (IModel)this.TransactionContext.GetResource("test.child");

            var id = childModel.CreateInternal(this.TransactionContext, child);

            childModel.WriteInternal(this.TransactionContext, id, child);
        }
    }
}
