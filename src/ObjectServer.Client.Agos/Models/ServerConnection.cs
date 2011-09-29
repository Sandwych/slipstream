using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

using ObjectServer.Client;

namespace ObjectServer.Client.Agos.Models
{
    public sealed class ServerConnection : IDisposable
    {
        private static volatile ServerConnection s_instance;
        private static readonly object syncRoot = new object();
        private static readonly object clientLock = new object();

        private ObjectServerClient client;

        private ServerConnection() { }

        ~ServerConnection()
        {
            this.Dispose();
        }

        public Task ConnectAsync(Uri uri)
        {
            Debug.Assert(this.client != null);

            lock (clientLock)
            {
                this.client = new ObjectServerClient(uri);
            }
            var tcs = new TaskCompletionSource<Version>();
            this.client.GetVersionAsync().ContinueWith(tk =>
            {
                tcs.SetResult(tk.Result);
            });

            return tcs.Task;
        }

        public void BeginConnect(Uri uri, Action<Exception> resultCallback)
        {
            this.client.BeginGetVersion((ver, error) =>
            {
                resultCallback(error);
            });
        }

        public void Close()
        {
            lock (clientLock)
            {
                this.client = null;
            }
        }

        public static ServerConnection Current
        {
            get
            {
                if (s_instance == null)
                {
                    lock (syncRoot)
                    {
                        if (s_instance == null)
                            s_instance = new ServerConnection();
                    }
                }

                return s_instance;
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (this.client != null)
            {
                lock (clientLock)
                {
                    this.client = null;
                }
            }
        }

        #endregion
    }
}
