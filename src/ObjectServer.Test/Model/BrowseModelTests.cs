using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;

using NUnit.Framework;

namespace ObjectServer.Model.Test
{
    [TestFixture]
    public class BrowseModelTests : ServiceContextTestCaseBase
    {
        [Test]
        public void CanBrowseManyToOneAndOneToManyFields()
        {
            var masterModel = this.GetResource("test.master");
            var childModel = this.GetResource("test.child");

            var masterPropBag = new Dictionary<string, object>()
            {
                { "name", "master-obj" },
            };
            var masterId = (long)masterModel.Create(masterPropBag);
            var childRecord = new Dictionary<string, object>()
            {
                { "name", "child-obj" },
                { "master", masterId },
            };
            var childId = (long)childModel.Create(childRecord);

            dynamic dynamicChild = childModel.Browse(childId);
            Assert.AreEqual("master-obj", dynamicChild.master.name);

            dynamic dynamicMaster = masterModel.Browse(masterId);
            Assert.AreEqual(1, dynamicMaster.children.Length);
            Assert.AreEqual("child-obj", dynamicMaster.children[0].name);
        }

    }
}
