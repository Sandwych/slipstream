using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

using ObjectServer.Client;

namespace ObjectServer.Client.Agos.Models
{
    public sealed class ServerConnection : IServerConnection
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
                        {
                            s_instance = new ServerConnection();
                        }
                    }
                }

                return s_instance;
            }
        }

        public ObjectServer.Client.ObjectServerClient Client
        {
            get
            {
                Debug.Assert(this.client != null);
                return this.Client;
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
