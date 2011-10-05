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
        public void CanBrowseManyToOneAndOneToManyFields()
        {
            var masterModel = this.GetResource("test.master");
            var childModel = this.GetResource("test.child");

            var masterPropBag = new Dictionary<string, object>()
            {
                { "name", "master-obj" },
            };
            var masterId = (long)masterModel.Create(this.TransactionContext, masterPropBag);
            var childRecord = new Dictionary<string, object>()
            {
                { "name", "child-obj" },
                { "master", masterId },
            };
            var childId = (long)childModel.Create(this.TransactionContext, childRecord);

            dynamic dynamicChild = childModel.Browse(this.TransactionContext, childId);
            Assert.AreEqual("master-obj", dynamicChild.master.name);

            dynamic dynamicMaster = masterModel.Browse(this.TransactionContext, masterId);
            Assert.AreEqual(1, dynamicMaster.children.Length);
            Assert.AreEqual("child-obj", dynamicMaster.children[0].name);
        }

        [Test]
        public void CanBrowseReferenceField()
        {
            this.ClearTestModelTable();
            this.ClearMasterAndChildTable();

            var masterModel = this.GetResource("test.master");
            var childModel = this.GetResource("test.child");
            var testModel = this.GetResource("test.test_model");

            //创建 Master 与 Child 测试数据
            dynamic masterRecord1 = new ExpandoObject();
            masterRecord1.name = "master1";
            var masterId1 = masterModel.Create(this.TransactionContext, masterRecord1);

            dynamic childRecord1 = new ExpandoObject();
            childRecord1.name = "child1";
            var childId1 = childModel.Create(this.TransactionContext, childRecord1);

            //创建测试数据 TestModel
            dynamic testRecord1 = new ExpandoObject();
            testRecord1.name = "test1";
            testRecord1.address = "address1";
            testRecord1.reference_field = new object[] { "test.master", masterId1 };
            var testId1 = testModel.Create(this.TransactionContext, testRecord1);

            dynamic obj = testModel.Browse(this.TransactionContext, testId1);

            Assert.AreEqual("master1", obj.reference_field.name);
        }
    }
}
