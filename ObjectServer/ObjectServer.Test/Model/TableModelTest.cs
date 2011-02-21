using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;


using NUnit.Framework;

using ObjectServer.Model;

namespace ObjectServer.Model.Test
{

    [TestFixture]
    public class ModelBaseTest
    {

        IService proxy = new LocalService();

        [TestFixtureSetUp]
        public void Init()
        {
            ObjectServerStarter.Initialize();
        }

        [Test]
        public void CRUD_model()
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
            Console.WriteLine(data);
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
            var children = (long[])master["children"];

            Assert.AreEqual(1, children.Length);
            Assert.AreEqual(childId, children[0]);
        }

        [Test]
        public void Many_to_many_fields()
        {
            var groupRecord = new Dictionary<string, object>()
            {
                { "name", "group" },
            };

            var groupId1 = (long)proxy.CreateModel("objectserver", "core.group", groupRecord);
            var groupId2 = (long)proxy.CreateModel("objectserver", "core.group", groupRecord);
            var groupIds = new object[] { groupId1, groupId2 };

            var userRecord = new Dictionary<string, object>()
            {
                { "name", "user" },
            };

            var userRecord1 = new Dictionary<string, object>()
            {
                { "name", "user1" },
                { "groups", new object[] { groupId1, groupId2 } },
            };

            var userId1 = (long)proxy.CreateModel("objectserver", "core.user", userRecord1);
            var userId2 = (long)proxy.CreateModel("objectserver", "core.user", userRecord);
            var userId3 = (long)proxy.CreateModel("objectserver", "core.user", userRecord);
            var userIds = new object[] { userId1, userId2, userId3 };

            var users = proxy.ReadModel("objectserver", "core.user", 
                new object[] { userId1 }, new object[] { "name", "groups" });
            Assert.AreEqual(1, users.Length);
            var user1 = users[0];
            Assert.IsInstanceOf<object[]>(user1["groups"]);
            var groupsField = (object[])user1["groups"];
            Assert.AreEqual(2, groupsField.Length);
        }

    }
}
