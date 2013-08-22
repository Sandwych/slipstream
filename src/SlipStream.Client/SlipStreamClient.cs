using System;
using System.Net;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

using SlipStream.Client.Model;
using Sandwych;
using Sandwych.Utility;

using Sandwych.Json;

namespace SlipStream.Client
{
    public class SlipStreamClient : ISlipStreamClient
    {
        public const string ServicePath = @"/jsonrpc";

        private JsonRpcClient jsonRpcClient;

        public SlipStreamClient(Uri uri)
        {
            this.ServerAddress = uri;
            this.Uri = new Uri(uri, ServicePath);
            this.jsonRpcClient = new JsonRpcClient(this.Uri);
        }

        public string SessionToken { get; private set; }
        public string LoggedUserName { get; private set; }
        public string LoggedDatabase { get; private set; }
        public Uri Uri { get; private set; }
        public Uri ServerAddress { get; private set; }

        public bool Logged { get { return !string.IsNullOrEmpty(this.SessionToken); } }

        public void GetVersion(Action<Version, Exception> resultCallback)
        {
            this.jsonRpcClient.Invoke("getVersion", null, (result, error) =>
            {
                var version = Version.Parse((string)result);
                resultCallback(version, error);
            });
        }

        public void ListDatabases(Action<string[], Exception> resultCallback)
        {
            this.jsonRpcClient.Invoke("listDatabases", null, (o, error) =>
            {
                object[] objs = (object[])o;
                var result = new string[objs.Length];
                for (int i = 0; i < result.Length; i++)
                {
                    result[i] = (string)objs[i];
                }

                resultCallback(result, error);
            });
        }

        public void CreateDatabase(
            string serverPassword, string dbName, string adminPassword, Action<Exception> resultCallback)
        {
            var hashedRootPassword = serverPassword.Trim().ToSha();
            var args = new object[] { hashedRootPassword, dbName.Trim(), adminPassword.Trim() };
            this.jsonRpcClient.Invoke("createDatabase", args, (result, error) =>
            {
                resultCallback(error);
            });
        }

        public void DeleteDatabase(
            string serverPassword, string dbName, Action<Exception> resultCallback)
        {
            var hashedRootPassword = serverPassword.Trim().ToSha();
            var args = new object[] { hashedRootPassword, dbName.Trim() };
            this.jsonRpcClient.Invoke("deleteDatabase", args, (result, error) =>
            {
                resultCallback(error);
            });
        }

        public void LogOn(
            string dbName, string userName, string password, Action<string, Exception> resultCallback)
        {
            Debug.Assert(!this.Logged);

            this.jsonRpcClient.Invoke("logOn", new object[] { dbName, userName, password }, (result, error) =>
            {
                var sid = (string)result;
                this.SessionToken = sid;
                this.LoggedDatabase = dbName;
                this.LoggedUserName = userName;
                resultCallback(sid, error);
            });

        }

        public void LogOff(Action<Exception> resultCallback)
        {
            Debug.Assert(this.Logged);

            var args = new object[] { this.LoggedDatabase, this.SessionToken };
            this.jsonRpcClient.Invoke("logOff", args, (result, error) =>
            {
                this.SessionToken = null;
                resultCallback(error);
            });
        }

        public void Execute(
            string objectName, string method, object[] parameters, Action<object, Exception> resultCallback)
        {
            Debug.Assert(this.Logged);

            var args = new object[] { this.LoggedDatabase, this.SessionToken, objectName, method, parameters };
            this.jsonRpcClient.Invoke("execute", args, (result, error) =>
            {
                resultCallback(result, error);
            });
        }

        public void CountModel(
            string objectName, object[][] constraints, Action<long, Exception> resultCallback)
        {
            Debug.Assert(this.Logged);

            var args = new object[] { constraints };
            this.Execute(objectName, "Count", args, (result, error) =>
            {
                resultCallback((long)result, error);
            });
        }

        public void SearchModel(
            string objectName, object[][] constraints, object[][] order, long offset, long limit,
            Action<long[], Exception> resultCallback)
        {
            Debug.Assert(this.Logged);

            var args = new object[] { constraints, order, offset, limit };
            this.Execute(objectName, "Search", args, (result, error) =>
            {
                var ids = ((object[])result).Select(id => (long)id).ToArray();
                resultCallback(ids, error);
            });
        }

        public void ReadModel(
            string objectName, IEnumerable<long> ids, IEnumerable<string> fields,
            Action<Dictionary<string, object>[], Exception> resultCallback)
        {
            Debug.Assert(this.Logged);

            var args = new object[] { ids, fields };
            this.Execute(objectName, "Read", args, (o, error) =>
            {
                var objs = (object[])o;
                var records = objs.Select(r => (Dictionary<string, object>)r);
                resultCallback(records.ToArray(), error);
            });
        }

        public void CreateModel(
            string objectName, IDictionary<string, object> fields, Action<long, Exception> resultCallback)
        {
            Debug.Assert(this.Logged);

            var args = new object[] { fields };
            this.Execute(objectName, "Create", args, (o, error) =>
            {
                resultCallback((long)o, error);
            });
        }

        public void WriteModel(
            string objectName, long id, IDictionary<string, object> fields, Action<Exception> resultCallback)
        {
            Debug.Assert(this.Logged);

            var args = new object[] { id, fields };
            this.Execute(objectName, "Write", args, (o, error) =>
            {
                resultCallback(error);
            });
        }

        public void DeleteModel(
            string objectName, long[] ids, Action<Exception> resultCallback)
        {
            Debug.Assert(this.Logged);

            var args = new object[] { ids };
            this.Execute(objectName, "Delete", args, (o, error) =>
            {
                resultCallback(error);
            });
        }

        public void ReadAllMenus(Action<MenuEntity[], Exception> resultCallback)
        {
            Debug.Assert(this.Logged);

            this.SearchModel("core.menu", null, null, 0, 0,
                (ids, searchError) =>
                {
                    if (searchError != null)
                    {
                        resultCallback(null, searchError);
                        return;
                    }

                    this.ReadModel("core.menu", ids, null, (records, readError) =>
                    {
                        if (readError != null)
                        {
                            resultCallback(null, readError);
                            return;
                        }

                        var menus = records.Select(r => new MenuEntity(r));

                        resultCallback(menus.ToArray(), readError);
                    });
                });
        }

    }
}
