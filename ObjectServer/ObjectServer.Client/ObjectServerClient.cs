using System;
using System.Net;
using System.Text;

namespace ObjectServer.Client
{
    public class ObjectServerClient
    {
        private JsonRpcClient jsonRpcClient;

        public ObjectServerClient(Uri uri)
        {
            this.jsonRpcClient = new JsonRpcClient(uri);
        }

        public void GetVersion(Action<Version> resultHandler)
        {
            this.jsonRpcClient.InvokeAsync("GetVersion", null, o =>
            {
                var version = Version.Parse((string)o);
                resultHandler(version);
            });
        }

        public void ListDatabases(Action<dynamic> resultHandler)
        {
            this.jsonRpcClient.InvokeAsync("ListDatabases", null, o =>
            {
                resultHandler(o);
            });
        }

    }
}
