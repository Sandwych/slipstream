using System;
using System.Net;
using System.Threading;
using System.Diagnostics;

using SlipStream.Client;

namespace SlipStream.Client.Agos.Models
{
    public sealed class ServerConnection : IServerConnection
    {
        private static volatile ServerConnection s_instance;
        private static readonly object syncRoot = new object();
        private static readonly object clientLock = new object();

        private SlipStreamClient client;

        private ServerConnection() { }

        ~ServerConnection()
        {
            this.Dispose();
        }

        public void BeginConnect(Uri uri, System.Action<Exception> resultCallback)
        {
            this.client.GetVersion((ver, error) =>
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

        public SlipStream.Client.SlipStreamClient Client
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
