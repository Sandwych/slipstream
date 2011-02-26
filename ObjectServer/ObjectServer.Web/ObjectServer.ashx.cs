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
        private IService service = new ServiceDispatcher();

        [JsonRpcMethod]
        public string LogOn(string dbName, string userName, string password)
        {
            return this.service.LogOn(dbName, userName, password);
        }

        [JsonRpcMethod]
        public void LogOff(string sessionId)
        {
            this.service.LogOff(sessionId);
        }

        [JsonRpcMethod]
        public string GetVersion()
        {
            return this.service.GetVersion();
        }

        [JsonRpcMethod]
        public object Execute(string sessionId, string objectName, string method, object[] parameters)
        {
            return this.service.Execute(sessionId, objectName, method, parameters);
        }

    }
}
