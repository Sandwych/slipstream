using System;
using System.Net;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

using ObjectServer.Client.Model;

namespace ObjectServer.Client
{
    public class ObjectServerClient : IRemoteService
    {
        public const string ServicePath = @"/jsonrpc";

        private JsonRpcClient jsonRpcClient;

        public ObjectServerClient(Uri uri)
        {
            this.ServerAddress = uri;
            this.Uri = new Uri(uri, ServicePath);
            this.jsonRpcClient = new JsonRpcClient(this.Uri);
        }

        public string SessionId { get; private set; }
        public string LoggedUserName { get; private set; }
        public string LoggedDatabase { get; private set; }
        public Uri Uri { get; private set; }
        public Uri ServerAddress { get; private set; }

        bool Logged { get { return !string.IsNullOrEmpty(this.SessionId); } }

        public void GetVersion(Action<Version> resultCallback)
        {
            this.jsonRpcClient.InvokeSync("getVersion", null, o =>
            {
                var version = Version.Parse((string)o);
                resultCallback(version);
            });
        }

        public void ListDatabases(Action<string[]> resultCallback)
        {
            this.jsonRpcClient.InvokeSync("listDatabases", null, o =>
            {
                object[] objs = (object[])o;
                var result = new string[objs.Length];
                for (int i = 0; i < result.Length; i++)
                {
                    result[i] = (string)objs[i];
                }

                resultCallback(result);
            });
        }

        public void LogOn(
            string dbName, string userName, string password, Action<string> resultCallback)
        {
            Debug.Assert(!this.Logged);

            this.jsonRpcClient.InvokeSync("logOn", new object[] { dbName, userName, password }, o =>
            {
                //TODO  线程安全
                var sid = (string)o;
                if (!string.IsNullOrEmpty(sid))
                {
                    this.SessionId = sid;
                    this.LoggedDatabase = dbName;
                    this.LoggedUserName = userName;
                }
                resultCallback(this.SessionId);
            });

        }

        public void LogOff(Action resultCallback)
        {
            Debug.Assert(this.Logged);

            this.jsonRpcClient.InvokeSync("logOff", null, o =>
            {
                this.SessionId = null;
                resultCallback();
            });
        }

        public void Execute(
            string objectName, string method, object[] parameters, Action<object> resultCallback)
        {
            Debug.Assert(this.Logged);

            var args = new object[] { this.SessionId, objectName, method, parameters };
            this.jsonRpcClient.InvokeSync("execute", args, o =>
            {
                resultCallback(o);
            });
        }

        public void SearchModel(
            string objectName, object[][] domain, object[][] order, long offset, long limit, Action<long[]> resultCallback)
        {
            Debug.Assert(this.Logged);

            var args = new object[] { domain, order, offset, limit };
            this.Execute(objectName, "Search", args, o =>
            {
                resultCallback(((object[])o).Select(id => (long)id).ToArray());
            });
        }

        public void ReadModel(
            string objectName, IEnumerable<long> ids, IEnumerable<string> fields,
            Action<Dictionary<string, object>[]> resultCallback)
        {
            Debug.Assert(this.Logged);

            var args = new object[] { ids, fields };
            this.Execute(objectName, "Read", args, o =>
            {
                var objs = (object[])o;
                var records = objs.Select(r => (Dictionary<string, object>)r);
                resultCallback(records.ToArray());
            });
        }

        public void WriteModel(
            string objectName, long id, IDictionary<string, object> fields, Action resultCallback)
        {
            Debug.Assert(this.Logged);

            var args = new object[] { id, fields };
            this.Execute(objectName, "Write", args, o =>
            {
                resultCallback();
            });
        }

        public void CreateModel(
            string objectName, IDictionary<string, object> fields, Action<long> resultCallback)
        {
            Debug.Assert(this.Logged);

            var args = new object[] { fields };
            this.Execute(objectName, "Create", args, o =>
            {
                resultCallback((long)o);
            });
        }


        public void ReadAllMenus(Action<Menu[]> resultCallback)
        {
            Debug.Assert(this.Logged);

            this.SearchModel("core.menu", null, null, 0, 0,
                ids =>
                {
                    this.ReadModel("core.menu", ids, null, records =>
                        {
                            var menus = records.Select(r => new Menu(r));

                            resultCallback(menus.ToArray());
                        });
                });
        }

    }
}
