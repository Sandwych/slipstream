using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data;

using NUnit.Framework;

using Newtonsoft.Json;
using ObjectServer.Json;

namespace ObjectServer.Test
{
    [TestFixture]
    public class JsonTest
    {
        [Test]
        public void Test_serialization_of_property_bag()
        {
            ObjectServerStarter.Initialize();

            var dt = new Dictionary<string, object>();
            dt.Add("id", 1111);
            dt.Add("name", "aaaaaaaa");

            var str = PlainJsonConvert.SerializeObject(dt);

            var dt2 = (IDictionary<string, object>)PlainJsonConvert.DeserializeObject(str);

            Assert.NotNull(dt2);
            Assert.AreEqual(2, dt2.Count);
            Assert.AreEqual(1111, (long)dt2["id"]);
        }

        [Test]
        public void Test_serialization_of_array()
        {
            ObjectServerStarter.Initialize();

            var a1 = new object[] { 123, "aaaa", 12.5M };
            var json = PlainJsonConvert.SerializeObject(a1);

            var a2 = (object[])PlainJsonConvert.DeserializeObject(json);

            Assert.NotNull(a2);
            Assert.AreEqual(123, a2[0]);
            Assert.AreEqual("aaaa", a2[1]);
        }

        [Test]
        public void Test_serialization_complex_json()
        {
            ObjectServerStarter.Initialize();

            var json = "[0, 1, 2, [1,2,3], {\"aaa\": [1,2]}, [{b: 5}], {1: 1.1, 2: 2.2} ]";
            var array = (object[])PlainJsonConvert.DeserializeObject(json);

            Assert.IsInstanceOf<object[]>(array[3]);

            Assert.IsInstanceOf<Dictionary<string, object>>(array[4]);
            var dict1 = (Dictionary<string, object>)array[4];
            Assert.AreEqual("aaa", dict1.Keys.ToArray()[0]);

            Assert.IsInstanceOf<object[]>(dict1["aaa"]);
            var array2 = (object[])dict1["aaa"];
            Assert.AreEqual(2, array2.Length);
            Assert.AreEqual(1, array2[0]);
            Assert.AreEqual(2, array2[1]);

            Assert.IsInstanceOf<Dictionary<string, object>>(array[6]);
            var dict3 = (Dictionary<string, object>)array[6];
            Assert.Contains("1", dict3.Keys);
            Assert.Contains("2", dict3.Keys);

        }

        [Test]
        public void Serialize_dynamic()
        {
            var json = "{aaa:123, bbb:456, ccc:[1,2,3]}";
            dynamic jobject = JsonConvert.DeserializeObject(json);
            Assert.AreEqual(123, (int)jobject.aaa);
            Assert.AreEqual(3, jobject.ccc.Count);
        }
    }
}
