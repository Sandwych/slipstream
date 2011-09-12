using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;

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
            var masterId = (long)this.Service.Execute(this.SessionId, "test.master", "Create", masterPropBag);
            var childPropBag = new Dictionary<string, object>()
            {
                { "name", "child-obj" },
                { "master", masterId },
            };
            var childId = (long)this.Service.Execute(this.SessionId, "test.child", "Create", childPropBag);

            dynamic childModel = this.ServiceScope.GetResource("test.child");
            dynamic dynamicChild = childModel.Browse(this.ServiceScope, childId);
            Assert.AreEqual("master-obj", dynamicChild.master.name);

            IModel masterModel = (IModel)this.ServiceScope.GetResource("test.master");
            dynamic dynamicMaster = masterModel.Browse(this.ServiceScope, masterId);
            Assert.AreEqual(1, dynamicMaster.children.Length);
            Assert.AreEqual("child-obj", dynamicMaster.children[0].name);
        }

        [Test]
        public void Test_browse_reference_field()
        {
            this.ClearTestModelTable();
            this.ClearMasterAndChildTable();

            //创建 Master 与 Child 测试数据
            dynamic masterRecord1 = new ExpandoObject();
            masterRecord1.name = "master1";
            var masterId1 = this.Service.Execute(this.SessionId, "test.master", "Create", masterRecord1);

            dynamic childRecord1 = new ExpandoObject();
            childRecord1.name = "child1";
            var childId1 = this.Service.Execute(this.SessionId, "test.child", "Create", childRecord1);

            //创建测试数据 TestModel
            dynamic testRecord1 = new ExpandoObject();
            testRecord1.name = "test1";
            testRecord1.address = "address1";
            testRecord1.reference_field = new object[] { "test.master", masterId1 };
            var testId1 = this.Service.Execute(this.SessionId, "test.test_model", "Create", testRecord1);

            var model = (IModel)this.ServiceScope.GetResource("test.test_model");
            dynamic obj = model.Browse(this.ServiceScope, testId1);

            Assert.AreEqual("master1", obj.reference_field.name);
        }
    }
}
