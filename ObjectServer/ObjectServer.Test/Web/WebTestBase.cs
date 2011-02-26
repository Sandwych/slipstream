using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Net;
using System.Threading;
using System.IO;

using NUnit.Framework;

using ObjectServer.Json;

namespace ObjectServer
{
    public abstract class WebTestBase
    {
        [TestFixtureSetUp()]
        public virtual void InitFramework()
        {
            var args = new object[] { "objectserver", "root", "root" };
            var result = JsonRpc("LogOn", args);
            this.SessionId = (string)result["result"];
        }

        public string SessionId { get; private set; }

        public static IDictionary<string, object> JsonRpc(string method, params object[] args)
        {
            var id = Guid.NewGuid();

            var requestData = new Dictionary<string, object>();
            requestData["id"] = id;
            requestData["method"] = method;
            requestData["params"] = args;

            var requestContent = PlainJsonConvert.SerializeObject(requestData);

            // Prepare web request...
            HttpWebRequest httpRequest =
                   (HttpWebRequest)WebRequest.Create("http://localhost:9287/ObjectServer.ashx");

            httpRequest.Method = "POST";
            httpRequest.ContentType = "text/json";

            using (var stream = httpRequest.GetRequestStream())
            using (var writer = new StreamWriter(stream, Encoding.UTF8))
            {
                writer.Write(requestContent);
                writer.Close();
            }

            HttpWebResponse response = (HttpWebResponse)httpRequest.GetResponse();
            using (var responseStream = response.GetResponseStream())
            {
                var result = (IDictionary<string, object>)PlainJsonConvert.Deserialize(responseStream);
                responseStream.Close();

                //TODO 对比两个 ID 是否相同

                return result;
            }
        }

    }
}
