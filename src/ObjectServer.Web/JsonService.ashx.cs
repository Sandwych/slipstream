using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Web.SessionState;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ObjectServer.Web
{
    /// <summary>
    /// $codebehindclassname$ 的摘要说明
    /// </summary>
    [WebService()]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    public sealed class JsonService : JsonRpcHandler //IReadOnlySessionState 
    {
        private IExportedService service = ObjectServerStarter.ExportedService;

        public JsonService()
        {
            //遍历并注册所有服务对象为 <uri>/objects/*
        }

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
        public string[] ListDatabases()
        {
            return this.service.ListDatabases();
        }

        [JsonRpcMethod]
        public object Execute(string sessionId, string objectName, string method, object[] parameters)
        {
            /*
            var cookie = HttpContext.Current.Request.Cookies.Get("session-id");
            System.Diagnostics.Debug.WriteLine(cookie.Value);
             */

            return this.service.Execute(sessionId, objectName, method, parameters);
        }

    }
}
