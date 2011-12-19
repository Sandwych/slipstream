using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;

using NUnit.Framework;

using ObjectServer.Model;

namespace ObjectServer.Model.Fields
{
    [TestFixture]
    public class ReferenceFieldTests : ServiceContextTestCaseBase
    {
        public void PrepareTestingData()
        {
        }


        [Test]
        public void CanCrudReferenceField()
        {
            this.ClearTestModelTable();
            this.ClearMasterAndChildTable();

            var masterModel = this.GetResource("test.master");
            var childModel = this.GetResource("test.child");
            var testModel = this.GetResource("test.test_model");

            //创建 Master 与 Child 测试数据
            dynamic masterRecord1 = new ExpandoObject();
            masterRecord1.name = "master1";
            var masterId1 = masterModel.Create(masterRecord1);

            dynamic childRecord1 = new ExpandoObject();
            childRecord1.name = "child1";
            var childId1 = childModel.Create(childRecord1);

            //创建测试数据 TestModel
            dynamic testRecord1 = new ExpandoObject();
            testRecord1.name = "test1";
            testRecord1.address = "address1";
            testRecord1.reference_field = new object[] { "test.master", masterId1 };
            var testId1 = testModel.Create(testRecord1);

            dynamic testRecord2 = new ExpandoObject();
            testRecord2.name = "test2";
            testRecord2.address = "address2";
            testRecord2.reference_field = new object[] { "test.child", childId1 };
            var testId2 = testModel.Create(testRecord2);

            var testIds = new object[] { testId1, testId2 };
            var fields = new object[] { "reference_field" };
            var testRecords = testModel.Read(testIds, fields);

            Assert.AreEqual(2, testRecords.Length);
            Assert.IsInstanceOf(typeof(object[]), testRecords[0]["reference_field"]);
            Assert.IsInstanceOf(typeof(object[]), testRecords[1]["reference_field"]);

            var referenceField1 = (object[])testRecords[0]["reference_field"];
            var referenceField2 = (object[])testRecords[1]["reference_field"];

            Assert.AreEqual(3, referenceField1.Length); //必须是三元组
            Assert.AreEqual(3, referenceField2.Length); //必须是三元组

            Assert.AreEqual("test.master", referenceField1[0]); //三元组第一个元素是 model 名称
            Assert.AreEqual(masterId1, referenceField1[1]); //第二个元素是关联的 id
            Assert.AreEqual("master1", referenceField1[2]); //第三个元素是关联的 record 的 name 字段值

            Assert.AreEqual("test.child", referenceField2[0]); //三元组第一个元素是 model 名称
            Assert.AreEqual(childId1, referenceField2[1]); //第二个元素是关联的 id
            Assert.AreEqual("child1", referenceField2[2]); //第三个元素是关联的 record 的 name 字段值

            //测试浏览 Reference 字段
            dynamic test1 = testModel.Browse(testId1);
            Assert.AreEqual("master1", test1.reference_field.name);
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
            var masterId1 = masterModel.Create(masterRecord1);

            dynamic childRecord1 = new ExpandoObject();
            childRecord1.name = "child1";
            var childId1 = childModel.Create(childRecord1);

            //创建测试数据 TestModel
            dynamic testRecord1 = new ExpandoObject();
            testRecord1.name = "test1";
            testRecord1.address = "address1";
            testRecord1.reference_field = new object[] { "test.master", masterId1 };
            var testId1 = testModel.Create(testRecord1);

            dynamic obj = testModel.Browse(testId1);

            Assert.AreEqual("master1", obj.reference_field.name);
        }

    }
}
