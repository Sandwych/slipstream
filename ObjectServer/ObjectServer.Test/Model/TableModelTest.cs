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
    public class ModelBaseTest : TestBase
    {
        IService proxy = new LocalService();

        [Test]
        public void Simple_fields_crud()
        {
            var modelName = "test.test_object";
            var dbName = "objectserver";

            var values = new Dictionary<string, object>
            {
                { "name", "sweet_name" },
                { "address", "my address" },
                { "field1", 123 },
                { "field2", 100 },
            };
            var id = proxy.CreateModel(dbName, modelName, values);
            Assert.True(id > 0);

            var domain1 = new object[][] { new object[] { "name", "=", "sweet_name" } };
            var foundIds = proxy.SearchModel(dbName, modelName, domain1, 0, 100);
            Assert.AreEqual(1, foundIds.Length);
            Assert.AreEqual(id, foundIds[0]);

            var newValues = new Dictionary<string, object> {
                { "name", "changed_name" },
            };
            proxy.WriteModel(dbName, modelName, id, newValues);

            var ids = new object[] { id };
            var data = proxy.ReadModel(dbName, modelName, ids, null);
            Assert.AreEqual(1, data.Length);
            Assert.AreEqual("changed_name", data[0]["name"]);
            Assert.AreEqual(223, data[0]["field3"]); //检测函数字段的计算是否正确


            proxy.DeleteModel(dbName, modelName, ids);

            foundIds = proxy.SearchModel(dbName, modelName, domain1, 0, 100);
            Assert.AreEqual(0, foundIds.Length);
        }

        [Test]
        public void Many_to_one_and_one_to_many_fields()
        {

            var masterPropBag = new Dictionary<string, object>()
            {
                { "name", "master-obj" },
            };
            var masterId = proxy.CreateModel("objectserver", "test.master", masterPropBag);

            var childPropBag = new Dictionary<string, object>()
            {
                { "name", "child-obj" },
                { "master", masterId },
            };

            var childId = (long)proxy.CreateModel("objectserver", "test.child", childPropBag);

            var ids = new object[] { childId };
            var rows = proxy.ReadModel("objectserver", "test.child", ids, null);
            var masterField = rows[0]["master"];
            Assert.AreEqual(typeof(RelatedField), masterField.GetType());
            var one2ManyField = (RelatedField)masterField;
            Assert.AreEqual(one2ManyField.Id, masterId);
            Assert.AreEqual(one2ManyField.Name, "master-obj");

            var masterFieldNames = new object[] { "name", "children" };
            var masterRows = proxy.ReadModel(
                "objectserver", "test.master",
                new object[] { masterId }, masterFieldNames);
            var master = masterRows[0];
            var children = (object[])master["children"];

            Assert.AreEqual(1, children.Length);
            Assert.AreEqual(childId, children[0]);
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

            var userId1 = (long)proxy.CreateModel("objectserver", "core.user", userRecord);
            var userId2 = (long)proxy.CreateModel("objectserver", "core.user", userRecord);
            var userId3 = (long)proxy.CreateModel("objectserver", "core.user", userRecord);
            var userId4 = (long)proxy.CreateModel("objectserver", "core.user", userRecord);
            var userId5 = (long)proxy.CreateModel("objectserver", "core.user", userRecord);

            var groupRecord = new Dictionary<string, object>()
            {
                { "name", "group" },
            };

            var groupId1 = (long)proxy.CreateModel("objectserver", "core.group", groupRecord);
            var groupId2 = (long)proxy.CreateModel("objectserver", "core.group", groupRecord);
            var groupId3 = (long)proxy.CreateModel("objectserver", "core.group", groupRecord);
            var groupId4 = (long)proxy.CreateModel("objectserver", "core.group", groupRecord);
            var groupIds = new object[] { groupId1, groupId2 };

            //设置user1 对应 group2, group3, group4
            //设置 user2  对应 group3 group4

            proxy.CreateModel("objectserver", "core.user_group",
                new Dictionary<string, object>() { { "uid", userId1 }, { "gid", groupId2 }, });
            proxy.CreateModel("objectserver", "core.user_group",
                new Dictionary<string, object>() { { "uid", userId1 }, { "gid", groupId3 }, });
            proxy.CreateModel("objectserver", "core.user_group",
                new Dictionary<string, object>() { { "uid", userId1 }, { "gid", groupId4 }, });


            proxy.CreateModel("objectserver", "core.user_group",
                new Dictionary<string, object>() { { "uid", userId2 }, { "gid", groupId3 }, });
            proxy.CreateModel("objectserver", "core.user_group",
                new Dictionary<string, object>() { { "uid", userId2 }, { "gid", groupId4 }, });


            var users = proxy.ReadModel("objectserver", "core.user",
                new object[] { userId1, userId2 }, new object[] { "name", "groups" });

            Assert.AreEqual(2, users.Length);
            var user1 = users[0];
            var user2 = users[1];

            Assert.IsInstanceOf<object[]>(user1["groups"]);
            var groups1 = (object[])user1["groups"];
            Assert.AreEqual(3, groups1.Length);
        }


        [Test]
        public void Nullable_one_to_many_field()
        {
            var master = new Dictionary<string, object>();

            var id = proxy.CreateModel("objectserver", "test.master", master);

            var masterRecords = proxy.ReadModel("objectserver", "test.master",
                new object[] { id }, new object[] { "name", "children" });
            var record = masterRecords[0];

            Assert.IsInstanceOf<object[]>(record["children"]);
            Assert.IsInstanceOf<DBNull>(record["name"]);
        }


        [Test]
        public void Nullable_many_to_one_field()
        {
            var nameFieldValue = "child_with_empty_master_field";
            var child = new Dictionary<string, object>()
            {
                { "name", nameFieldValue },
            };

            var id = proxy.CreateModel("objectserver", "test.child", child);

            var children = proxy.ReadModel("objectserver", "test.child",
                new object[] { id }, new object[] { "name", "master" });
            var record = children[0];

            Assert.IsInstanceOf<DBNull>(record["master"]);
            Assert.AreEqual(nameFieldValue, (string)record["name"]);

        }
    }
}
