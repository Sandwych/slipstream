using System;
using System.Net;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;

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

        public void BeginGetVersion(Action<Version, Exception> resultCallback)
        {
            this.jsonRpcClient.BeginInvoke("getVersion", null, (result, error) =>
            {
                var version = Version.Parse((string)result);
                resultCallback(version, error);
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

        public void BeginListDatabases(Action<string[], Exception> resultCallback)
        {
            this.jsonRpcClient.BeginInvoke("listDatabases", null, (o, error) =>
            {
                if (error != null)
                {
                    resultCallback(null, error);
                    return;
                }

                object[] objs = (object[])o;
                var result = new string[objs.Length];
                for (int i = 0; i < result.Length; i++)
                {
                    result[i] = (string)objs[i];
                }

                resultCallback(result, error);
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
            this.jsonRpcClient.BeginInvoke("createDatabase", args, o =>
            {
                resultCallback();
            });
        }

        public void BeginDeleteDatabase(string serverPassword, string dbName, Action resultCallback)
        {
            var hashedRootPassword = serverPassword.Trim().ToSha();
            var args = new object[] { hashedRootPassword, dbName.Trim() };
            this.jsonRpcClient.BeginInvoke("deleteDatabase", args, o =>
            {
                resultCallback();
            });
        }

        public void BeginLogOn(
            string dbName, string userName, string password, Action<string, Exception> resultCallback)
        {
            Debug.Assert(!this.Logged);

            this.jsonRpcClient.BeginInvoke("logOn", new object[] { dbName, userName, password }, (o, error) =>
            {
                //TODO  线程安全
                var sid = (string)o;
                if (!string.IsNullOrEmpty(sid))
                {
                    this.SessionId = sid;
                    this.LoggedDatabase = dbName;
                    this.LoggedUserName = userName;
                }
                resultCallback(this.SessionId, error);
            });

        }

        public void BeginLogOff(Action<Exception> resultCallback)
        {
            Debug.Assert(this.Logged);

            var args = new object[] { this.LoggedDatabase, this.SessionId };
            this.jsonRpcClient.BeginInvoke("logOff", args, (result, error) =>
            {
                this.SessionId = null;
                resultCallback(error);
            });
        }

        public void BeginExecute(
            string objectName, string method, object[] parameters, Action<object> resultCallback)
        {
            Debug.Assert(this.Logged);

            var args = new object[] { this.LoggedDatabase, this.SessionId, objectName, method, parameters };
            this.jsonRpcClient.BeginInvoke("execute", args, o =>
            {
                resultCallback(o);
            });
        }

        public void BeginExecute(
            string objectName, string method, object[] parameters, Action<object, Exception> resultCallback)
        {
            Debug.Assert(this.Logged);

            var args = new object[] { this.LoggedDatabase, this.SessionId, objectName, method, parameters };
            this.jsonRpcClient.BeginInvoke("execute", args, (result, error) =>
            {
                resultCallback(result, error);
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
            string objectName, object[][] constraints, Action<long, Exception> resultCallback)
        {
            Debug.Assert(this.Logged);

            var args = new object[] { constraints };
            this.BeginExecute(objectName, "Count", args, (result, error) =>
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
            this.BeginExecute(objectName, "Search", args, (result, error) =>
            {
                var ids = ((object[])result).Select(id => (long)id).ToArray();
                resultCallback(ids, error);
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

        public void ReadAllMenus(Action<MenuModel[]> resultCallback)
        {
            Debug.Assert(this.Logged);

            this.SearchModel("core.menu", null, null, 0, 0,
                (ids, error) =>
                {
                    this.ReadModel("core.menu", ids, null, records =>
                        {
                            var menus = records.Select(r => new MenuModel(r));

                            resultCallback(menus.ToArray());
                        });
                });
        }

    }
}
