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
    public class TableModelTest : LocalTestBase
    {
        [Test]
        public void Simple_fields_crud()
        {
            var modelName = "test.test_model";
            var dbName = this.SessionId;

            //Delete all records first
            var allIds = this.Service.SearchModel(this.SessionId, modelName, null, 0, 0);
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
            var foundIds = this.Service.SearchModel(this.SessionId, modelName, domain1, 0, 100);
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

            foundIds = this.Service.SearchModel(this.SessionId, modelName, domain1, 0, 100);
            Assert.AreEqual(0, foundIds.Length);
        }

        [Test]
        public void Create_and_read_related_fields()
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

            var ids = new object[] { childId };
            var rows = this.Service.ReadModel(this.SessionId, "test.child", ids, null);
            var masterField = rows[0]["master"];
            Assert.AreEqual(typeof(object[]), masterField.GetType());
            var one2ManyField = (object[])masterField;
            Assert.AreEqual(one2ManyField[0], masterId);
            Assert.AreEqual(one2ManyField[1], "master-obj");

            var masterFieldNames = new object[] { "name", "children" };
            var masterRows = this.Service.ReadModel(
                this.SessionId, "test.master",
                new object[] { masterId }, masterFieldNames);
            var master = masterRows[0];
            var children = (object[])master["children"];

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
        public void Many_to_many_fields()
        {
            var userRecord = new Dictionary<string, object>()
            {
                { "name", "user1" },
                { "login", "account" },
                { "password", "xxxxxx" },
                { "admin", true },
            };

            var userId1 = (long)this.Service.CreateModel(this.SessionId, "core.user", userRecord);
            var userId2 = (long)this.Service.CreateModel(this.SessionId, "core.user", userRecord);
            var userId3 = (long)this.Service.CreateModel(this.SessionId, "core.user", userRecord);
            var userId4 = (long)this.Service.CreateModel(this.SessionId, "core.user", userRecord);
            var userId5 = (long)this.Service.CreateModel(this.SessionId, "core.user", userRecord);

            var groupRecord = new Dictionary<string, object>()
            {
                { "name", "group" },
            };

            var groupId1 = (long)this.Service.CreateModel(this.SessionId, "core.group", groupRecord);
            var groupId2 = (long)this.Service.CreateModel(this.SessionId, "core.group", groupRecord);
            var groupId3 = (long)this.Service.CreateModel(this.SessionId, "core.group", groupRecord);
            var groupId4 = (long)this.Service.CreateModel(this.SessionId, "core.group", groupRecord);
            var groupIds = new object[] { groupId1, groupId2 };

            //设置user1 对应 group2, group3, group4
            //设置 user2  对应 group3 group4

            this.Service.CreateModel(this.SessionId, "core.user_group",
                new Dictionary<string, object>() { { "uid", userId1 }, { "gid", groupId2 }, });
            this.Service.CreateModel(this.SessionId, "core.user_group",
                new Dictionary<string, object>() { { "uid", userId1 }, { "gid", groupId3 }, });
            this.Service.CreateModel(this.SessionId, "core.user_group",
                new Dictionary<string, object>() { { "uid", userId1 }, { "gid", groupId4 }, });


            this.Service.CreateModel(this.SessionId, "core.user_group",
                new Dictionary<string, object>() { { "uid", userId2 }, { "gid", groupId3 }, });
            this.Service.CreateModel(this.SessionId, "core.user_group",
                new Dictionary<string, object>() { { "uid", userId2 }, { "gid", groupId4 }, });


            var users = this.Service.ReadModel(this.SessionId, "core.user",
                new object[] { userId1, userId2 }, new object[] { "name", "groups" });

            Assert.AreEqual(2, users.Length);
            var user1 = users[0];
            var user2 = users[1];

            Assert.IsInstanceOf<object[]>(user1["groups"]);
            var groups1 = (object[])user1["groups"];
            Assert.AreEqual(3, groups1.Length);
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

            Assert.IsInstanceOf<object[]>(record["children"]);
            Assert.AreEqual(0, ((object[])record["children"]).Length);
            Assert.IsInstanceOf<DBNull>(record["name"]);
        }


        [Test]
        public void Read_nullable_many_to_one_field()
        {
            var nameFieldValue = "child_with_empty_master_field";
            var child = new Dictionary<string, object>()
            {
                { "name", nameFieldValue },
            };

            var id = this.Service.CreateModel(this.SessionId, "test.child", child);

            var children = this.Service.ReadModel(this.SessionId, "test.child",
                new object[] { id }, new object[] { "name", "master" });
            var record = children[0];

            Assert.IsInstanceOf<DBNull>(record["master"]);
            Assert.AreEqual(nameFieldValue, (string)record["name"]);
        }
    }
}
