using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Data;
using System.Threading;

using ObjectServer.Data;

namespace ObjectServer
{
    internal sealed class DummyTransactionContext : ITransactionContext
    {
        private bool disposed = false;
        private readonly IResourceContainer resources;
        private readonly IDBContext db;
        private readonly int currentThreadID = Thread.CurrentThread.ManagedThreadId;

        public DummyTransactionContext(IDBContext db)
        {
            Debug.Assert(db != null);
            this.db = db;
            this.Session = Session.CreateSystemUserSession();
            Session.Put(db, this.Session);
            this.resources = Environment.DBProfiles.GetDBProfile(db.DatabaseName);
        }

        ~DummyTransactionContext()
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
                //删除系统 Session
                if (this.Session.IsSystemUser)
                {
                    Session.Remove(this.db, this.Session.ID);
                }

                this.disposed = true;
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        public bool Equals(ITransactionContext other)
        {
            throw new NotSupportedException("Invalid Equals invocation");
        }

        public IResource GetResource(string resName)
        {
            Debug.Assert(this.currentThreadID == Thread.CurrentThread.ManagedThreadId);
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
            Debug.Assert(this.currentThreadID == Thread.CurrentThread.ManagedThreadId);
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
                Debug.Assert(this.currentThreadID == Thread.CurrentThread.ManagedThreadId);
                Debug.Assert(this.db != null);
                if (this.disposed)
                {
                    throw new ObjectDisposedException("disposed");
                }
                return this.db;
            }
        }

        public IDbTransaction DBTransaction
        {
            get
            {
                throw new NotSupportedException();
            }
        }
    }
}
