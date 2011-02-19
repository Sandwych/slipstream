using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace ObjectServer.Test
{
    [TestFixture]
    public class JsonTest
    {

        [Test]
        public void TestJsonSerialize()
        {
            var js =
@"
[
    0,1,2, 
    [1,2,3], 
    {""a"": [1,2]},
    [{""b"": 5}]
]
";
            var jobjs = (JArray)JsonConvert.DeserializeObject(js);
            var objs = (object[])ConvertJsonToken(jobjs);
            var sub3 = (object[])objs[3];
            Console.WriteLine(sub3[0]);

            foreach (var obj in objs)
            {
                Console.WriteLine(obj.GetType().Name);
            }
            Console.WriteLine(objs);
        }

        public static object ConvertJsonToken(JToken tok)
        {
            object result;
            if (tok is JValue)
            {
                var val = tok as JValue;
                result = val.Value;
            }
            else if (tok is JObject)
            {
                var jo = tok as JObject;
                var dict = new Dictionary<string, object>(jo.Count);
                foreach (var prop in jo.Properties())
                {
                    dict[prop.Name] = ConvertJsonToken(jo[prop.Name]);
                }
                result = dict;
            }
            else if (tok is JArray)
            {
                var ja = tok as JArray;
                var array = new object[ja.Count];
                for (int i = 0; i < array.Length; i++)
                {
                    array[i] = ConvertJsonToken(ja[i]);
                }
                result = array;
            }
            else
            {
                throw new NotSupportedException();
            }

            return result;
        }
    }
}
