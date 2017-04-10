using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;

using Sandwych;
using NUnit.Framework;

using SlipStream.Entity;

namespace SlipStream.Entity.Fields
{
    [TestFixture(Category = "ORM")]
    public class ManyToOneFieldTests : ServiceContextTestCaseBase
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

            var childModel = (IEntity)this.GetResource("test.child");

            var id = childModel.CreateInternal(child);

            var children = childModel.ReadInternal(
                new long[] { id }, new string[] { "name", "master" });
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

            dynamic childModel = this.GetResource("test.child");

            var id = childModel.Create(child);

            var childRecord = childModel.Read(new long[] { id }, null)[0];
            child[AbstractEntity.VersionFieldName] = childRecord[AbstractEntity.VersionFieldName];

            childModel.Write(id, child);
        }
    }
}
