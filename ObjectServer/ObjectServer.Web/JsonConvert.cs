using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ObjectServer.Web
{
    public class JsonConvert
    {
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
