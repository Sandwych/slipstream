using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;

using NUnit.Framework;

using ObjectServer.Model;

namespace ObjectServer.Model.Test
{

    [TestFixture]
    public class TableModelTest : LocalTestCase
    {
        [Test]
        public void Simple_fields_crud()
        {
            var modelName = "test.test_model";
            var dbName = this.SessionId;

            //Delete all records first
            var allIds = this.Service.SearchModel(
                this.SessionId, modelName).Select(_ => (object)_).ToArray();
            if (allIds.Length > 0)
            {
                this.Service.DeleteModel(this.SessionId, modelName, allIds);
            }

            var values = new Dictionary<string, object>
            {
                { "name", "sweet_name" },
                { "address", "my address" },
                { "field1", 123 },
                { "field2", 100 },
            };
            var id = this.Service.CreateModel(this.SessionId, modelName, values);
            Assert.True(id > 0);

            var domain1 = new object[][] { new object[] { "name", "=", "sweet_name" } };
            var foundIds = this.Service.SearchModel(this.SessionId, modelName, domain1, null, 0, 100);
            Assert.AreEqual(1, foundIds.Length);
            Assert.AreEqual(id, foundIds[0]);

            var newValues = new Dictionary<string, object> {
                { "name", "changed_name" },
            };
            this.Service.WriteModel(this.SessionId, modelName, id, newValues);

            var ids = new object[] { id };
            var data = this.Service.ReadModel(this.SessionId, modelName, ids, null);
            Assert.AreEqual(1, data.Length);
            Assert.AreEqual("changed_name", data[0]["name"]);
            Assert.AreEqual(223, data[0]["field3"]); //检测函数字段的计算是否正确


            this.Service.DeleteModel(this.SessionId, modelName, ids);

            foundIds = this.Service.SearchModel(this.SessionId, modelName, domain1, null, 0, 100);
            Assert.AreEqual(0, foundIds.Length);
        }

        [Test]
        public void Create_and_read_related_fields()
        {
            this.ClearMasterAndChildTable();

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

            var ids = new object[] { childId };
            var rows = this.Service.ReadModel(this.SessionId, "test.child", ids, null);
            var masterField = rows[0]["master"];
            Assert.AreEqual(typeof(object[]), masterField.GetType());
            var one2ManyField = (object[])masterField;
            Assert.AreEqual(one2ManyField[0], masterId);
            Assert.AreEqual(one2ManyField[1], "master-obj");

            var masterFieldNames = new string[] { "name", "children" };
            var masterRows = this.Service.ReadModel(
                this.SessionId, "test.master",
                new object[] { masterId }, masterFieldNames);
            var master = masterRows[0];
            var children = (long[])master["children"];

            Assert.AreEqual(1, children.Length);
            Assert.AreEqual(childId, children[0]);

            //更新
            var masterId2 = (long)this.Service.CreateModel(this.SessionId, "test.master", masterPropBag);
            childPropBag["master"] = masterId2;
            this.Service.WriteModel(this.SessionId, "test.child", childId, childPropBag);

            var children2 = this.Service.ReadModel(this.SessionId, "test.child", new object[] { childId }, new object[] { "master" });
            var masterField3 = (object[])children2[0]["master"];
            Assert.AreEqual(masterId2, masterField3[0]);

        }


        [Test]
        public void Read_nullable_one_to_many_field()
        {
            var masterFields = new object[] { "name", "children" };
            var master = new Dictionary<string, object>();

            var id = this.Service.CreateModel(this.SessionId, "test.master", master);

            var masterRecords = this.Service.ReadModel(this.SessionId, "test.master",
                new object[] { id }, masterFields);
            var record = masterRecords[0];

            Assert.IsInstanceOf<long[]>(record["children"]);
            Assert.AreEqual(0, ((long[])record["children"]).Length);
            Assert.IsInstanceOf<DBNull>(record["name"]);
        }



        [Test]
        public void test_binary_field()
        {
            this.ClearTestModelTable();
            dynamic testModel = this.ResourceScope.GetResource("test.test_model");
            var fieldData = new byte[] { 33, 44, 55, 66, 77 };
            var record = new Dictionary<string, object>()
                {
                    { "name", "name1" },
                    { "address", "address1" },
                    { "binary_field", fieldData },
                };
            var id = (long)testModel.Create(this.ResourceScope, record);

            record = testModel.Read(this.ResourceScope, new object[] { id }, null)[0];

            var field = record["binary_field"] as byte[];
            Assert.NotNull(field);
            Assert.AreEqual(5, field.Length);
            Assert.AreEqual(fieldData[0], field[0]);
            Assert.AreEqual(fieldData[4], field[4]);
        }
    }
}
