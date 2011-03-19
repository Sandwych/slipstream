using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;

namespace ObjectServer.Model.Test
{
    [TestFixture]
    public class BrowseModelTests : LocalTestCase
    {

        [Test]
        public void Browse_many2one_and_one2many_fields()
        {
            var masterPropBag = new Dictionary<string, object>()
            {
                { "name", "master-obj" },
            };
            var masterId = this.Service.CreateModel(this.SessionId, "test.master", masterPropBag);
            var childPropBag = new Dictionary<string, object>()
            {
                { "name", "child-obj" },
                { "master", masterId },
            };
            var childId = (long)this.Service.CreateModel(this.SessionId, "test.child", childPropBag);

            dynamic childModel = this.ResourceScope.GetResource("test.child");
            dynamic dynamicChild = childModel.Browse(this.ResourceScope, childId);
            Assert.AreEqual("master-obj", dynamicChild.master.name);

            dynamic masterModel = this.ResourceScope.GetResource("test.master");
            dynamic dynamicMaster = masterModel.Browse(this.ResourceScope, masterId);
            Assert.AreEqual(1, dynamicMaster.children.Length);
            Assert.AreEqual("child-obj", dynamicMaster.children[0].name);
        }
    }
}
