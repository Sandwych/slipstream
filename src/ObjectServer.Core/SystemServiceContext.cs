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
        private readonly IResourceContainer resources;
        private readonly IDBContext db;

        public SystemServiceContext(IDBContext db)
        {
            Debug.Assert(db != null);
            this.db = db;
            this.Session = new Session(db.DatabaseName);
            this.resources = Environment.DBProfiles.GetDBProfile(db.DatabaseName);
        }

        ~SystemServiceContext()
        {
            this.Dispose(false);
        }

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

            return this.resources.GetResource(resName);
        }

        public int GetResourceDependencyWeight(string resName)
        {
            if (string.IsNullOrEmpty(resName))
            {
                throw new ArgumentNullException("resName");
            }

            if (this.disposed)
            {
                throw new ObjectDisposedException("disposed");
            }

            return this.resources.GetResourceDependencyWeight(resName);
        }

        public IDBContext DBContext
        {
            get
            {
                Debug.Assert(this.db != null);
                if (this.disposed)
                {
                    throw new ObjectDisposedException("disposed");
                }
                return this.db;
            }
        }
    }
}
