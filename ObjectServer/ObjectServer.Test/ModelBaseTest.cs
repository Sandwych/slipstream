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

namespace ObjectServer.Test
{

    [TestFixture]
    public class ModelBaseTest
    {
        public ModelBaseTest()
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
            var id = (long)proxy.Execute(
                dbName, modelName, "Create", values);
            Assert.True(id > 0);

            var foundIds = (long[])proxy.Execute(
                dbName, modelName, "Search", "(equal name 'sweet_name')", 0, 100);
            Assert.AreEqual(1, foundIds.Length);
            Assert.AreEqual(id, foundIds[0]);

            var newValues = new Dictionary<string, object> {
                { "name", "changed_name" },
            };
            proxy.Execute(dbName, modelName, "Write", id, newValues);

            var ids = new long[] { id };
            var data = (Dictionary<string, object>[])proxy.Execute(dbName, modelName, "Read", ids, null);
            Console.WriteLine(data);
            Assert.AreEqual(1, data.Length);
            Assert.AreEqual("changed_name", data[0]["name"]);
            Assert.AreEqual(223, data[0]["field3"]); //检测函数字段的计算是否正确


            proxy.Execute(dbName, modelName, "Delete", ids);

            foundIds = (long[])proxy.Execute(
                dbName, modelName, "Search", "(equal name 'sweet_name')", 0, 100);
            Assert.AreEqual(0, foundIds.Length);
        }
 
    }
}
