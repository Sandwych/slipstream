using System;
using System.Net;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using ObjectServer.Client.Model;
using ObjectServer.Utility;

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

        public bool Logged { get { return !string.IsNullOrEmpty(this.SessionId); } }

        public void BeginGetVersion(Action<Version> resultCallback)
        {
            this.jsonRpcClient.BeginInvoke("getVersion", null, (result) =>
            {
                var version = Version.Parse((string)result);
                resultCallback(version);
            });
        }

        public Task<Version> GetVersionAsync()
        {
            var tcs = new TaskCompletionSource<Version>();
            this.jsonRpcClient.InvokeAsync("getVersion", null)
                .ContinueWith(tk =>
                {
                    var VerStr = (string)tk.Result;
                    tcs.SetResult(Version.Parse(VerStr));
                });
            return tcs.Task;
        }

        public void BeginListDatabases(Action<string[]> resultCallback)
        {
            this.jsonRpcClient.BeginInvoke("listDatabases", null, (o) =>
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

        public Task<string[]> ListDatabasesAsync()
        {
            var tcs = new TaskCompletionSource<string[]>();
            this.jsonRpcClient.InvokeDyanmicAsync("listDatabases", null)
                .ContinueWith(tk =>
                {
                    if (tk.IsFaulted)
                    {
                        tcs.SetException(tk.Exception);
                        return;
                    }

                    var result = new string[tk.Result.Length];
                    for (int i = 0; i < tk.Result.Length; i++)
                    {
                        result[i] = (string)tk.Result[i];
                    }
                    tcs.SetResult(result);
                });

            return tcs.Task;
        }

        public void BeginCreateDatabase(string serverPassword, string dbName, string adminPassword, Action resultCallback)
        {
            var hashedRootPassword = serverPassword.Trim().ToSha();
            var args = new object[] { hashedRootPassword, dbName.Trim(), adminPassword.Trim() };
            this.jsonRpcClient.BeginInvoke("createDatabase", args, (result) =>
            {
                resultCallback();
            });
        }

        public void BeginDeleteDatabase(string serverPassword, string dbName, Action resultCallback)
        {
            var hashedRootPassword = serverPassword.Trim().ToSha();
            var args = new object[] { hashedRootPassword, dbName.Trim() };
            this.jsonRpcClient.BeginInvoke("deleteDatabase", args, (result) =>
            {
                resultCallback();
            });
        }

        public void BeginLogOn(
            string dbName, string userName, string password, Action<string> resultCallback)
        {
            Debug.Assert(!this.Logged);

            this.jsonRpcClient.BeginInvoke("logOn", new object[] { dbName, userName, password }, (result) =>
            {
                var sid = (string)result;
                this.SessionId = sid;
                this.LoggedDatabase = dbName;
                this.LoggedUserName = userName;
                resultCallback(sid);
            });

        }

        public void BeginLogOff(Action resultCallback)
        {
            Debug.Assert(this.Logged);

            var args = new object[] { this.LoggedDatabase, this.SessionId };
            this.jsonRpcClient.BeginInvoke("logOff", args, (result) =>
            {
                this.SessionId = null;
                resultCallback();
            });
        }

        public void BeginExecute(
            string objectName, string method, object[] parameters, Action<object> resultCallback)
        {
            Debug.Assert(this.Logged);

            var args = new object[] { this.LoggedDatabase, this.SessionId, objectName, method, parameters };
            this.jsonRpcClient.BeginInvoke("execute", args, (result) =>
            {
                resultCallback(result);
            });
        }

        public Task<object> ExecuteAsync(
            string objectName, string method, object[] parameters)
        {
            Debug.Assert(this.Logged);

            var tcs = new TaskCompletionSource<object>();
            var args = new object[] { this.LoggedDatabase, this.SessionId, objectName, method, parameters };
            this.jsonRpcClient.InvokeDyanmicAsync("execute", args)
                .ContinueWith(tk =>
                {
                    tcs.SetResult(tk.Result);
                });

            return tcs.Task;
        }

        public void CountModel(
            string objectName, object[][] constraints, Action<long> resultCallback)
        {
            Debug.Assert(this.Logged);

            var args = new object[] { constraints };
            this.BeginExecute(objectName, "Count", args, (result) =>
            {
                resultCallback((long)result);
            });
        }

        public void SearchModel(
            string objectName, object[][] constraints, object[][] order, long offset, long limit,
            Action<long[]> resultCallback)
        {
            Debug.Assert(this.Logged);

            var args = new object[] { constraints, order, offset, limit };
            this.BeginExecute(objectName, "Search", args, (result) =>
            {
                var ids = ((object[])result).Select(id => (long)id).ToArray();
                resultCallback(ids);
            });
        }

        public void ReadModel(
            string objectName, IEnumerable<long> ids, IEnumerable<string> fields,
            Action<Dictionary<string, object>[]> resultCallback)
        {
            Debug.Assert(this.Logged);

            var args = new object[] { ids, fields };
            this.BeginExecute(objectName, "Read", args, o =>
            {
                var objs = (object[])o;
                var records = objs.Select(r => (Dictionary<string, object>)r);
                resultCallback(records.ToArray());
            });
        }

        public void CreateModel(
            string objectName, IDictionary<string, object> fields, Action<long> resultCallback)
        {
            Debug.Assert(this.Logged);

            var args = new object[] { fields };
            this.BeginExecute(objectName, "Create", args, o =>
            {
                resultCallback((long)o);
            });
        }

        public void WriteModel(
            string objectName, long id, IDictionary<string, object> fields, Action resultCallback)
        {
            Debug.Assert(this.Logged);

            var args = new object[] { id, fields };
            this.BeginExecute(objectName, "Write", args, o =>
            {
                resultCallback();
            });
        }

        public void DeleteModel(
            string objectName, long[] ids, Action resultCallback)
        {
            Debug.Assert(this.Logged);

            var args = new object[] { ids };
            this.BeginExecute(objectName, "Delete", args, n =>
            {
                resultCallback();
            });
        }

        public void ReadAllMenus(Action<MenuEntity[]> resultCallback)
        {
            Debug.Assert(this.Logged);

            this.SearchModel("core.menu", null, null, 0, 0,
                (ids) =>
                {
                    this.ReadModel("core.menu", ids, null, records =>
                        {
                            var menus = records.Select(r => new MenuEntity(r));

                            resultCallback(menus.ToArray());
                        });
                });
        }

    }
}
