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

            var proxy = new LocalService();
            var values = new Dictionary<string, object>
            {
                { "name", "sweet_name" },
                { "address", "my address" },
                { "field1", 123 },
                { "field2", 100 },
            };
            var id = proxy.CreateModel(dbName, modelName, values);
            Assert.True(id > 0);

            var foundIds = proxy.SearchModel(dbName, modelName, "(equal name 'sweet_name')", 0, 100);
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

            foundIds = proxy.SearchModel(dbName, modelName, "(equal name 'sweet_name')", 0, 100);
            Assert.AreEqual(0, foundIds.Length);
        }

        [Test]
        public void Many_to_one_and_one_to_many_fields()
        {
            var proxy = new LocalService();

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
        }

    }
}
