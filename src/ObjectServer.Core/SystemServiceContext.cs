using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using ObjectServer.Data;

namespace ObjectServer
{
    internal class SystemServiceContext : IServiceContext
    {
        private bool disposed = false;

        public SystemServiceContext(IDBProfile db)
        {
            Debug.Assert(db != null);
            this.DBProfile = db;

            this.Session = new Session(db.DBContext.DatabaseName);
        }

        ~SystemServiceContext()
        {
            this.Dispose(false);
        }

        private IDBProfile DBProfile { get; set; }

        public Session Session { get; private set; }

        private void Dispose(bool isDisposing)
        {
            if (!this.disposed)
            {
                if (isDisposing)
                {
                    //处理托管资源
                }

                //处理非托管资源

                this.disposed = true;
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        public bool Equals(IServiceContext other)
        {
            throw new NotSupportedException("Invalid Equals invocation");
        }

        public IResource GetResource(string resName)
        {
            if (string.IsNullOrEmpty(resName))
            {
                throw new ArgumentNullException("resName");
            }

            if (this.disposed)
            {
                throw new ObjectDisposedException("disposed");
            }

            return this.DBProfile.GetResource(resName);
        }

        public IDBContext DBContext
        {
            get
            {
                Debug.Assert(this.DBProfile != null);
                if (this.disposed)
                {
                    throw new ObjectDisposedException("disposed");
                }
                return this.DBProfile.DBContext;
            }
        }
    }
}
