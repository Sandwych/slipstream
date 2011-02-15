using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ObjectServer.Web
{
    /// <summary>
    /// $codebehindclassname$ 的摘要说明
    /// </summary>
    [WebService()]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    public class ObjectServer : JsonRpcHandler
    {
        private IService service = new LocalService();

        [JsonRpcMethod(Name = "Execute")]
        public object Execute(string dbName, string objectName, string name, JArray parameters)
        {
            var paramObjs = parameters.Values<object>().ToArray();
            return this.service.Execute(dbName, objectName, name, paramObjs);
        }

    }
}
