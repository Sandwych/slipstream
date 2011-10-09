using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Reflection;
using System.Diagnostics;
using System.Threading;

using ObjectServer.Data;

namespace ObjectServer
{
    /// <summary>
    /// 但凡是需要 RPC 的方法都需要用此类包裹
    /// </summary>
    internal sealed class TransactionContext : ITransactionContext
    {
        private bool disposed = false;
        private readonly IResourceContainer resources;
        private readonly int currentThreadID;

        /// <summary>
        /// 安全的创建 Context，会检查 session 等
        /// </summary>
        /// <param name="sessionId"></param>
        public TransactionContext(string dbName, string sessionId)
        {
            if (string.IsNullOrEmpty(dbName))
            {
                throw new ArgumentNullException("dbName");
            }
            if (string.IsNullOrEmpty(sessionId))
            {
                throw new ArgumentNullException("sessionId");
            }

            this.currentThreadID = Thread.CurrentThread.ManagedThreadId;

            this.dbctx = DataProvider.CreateDataContext(dbName);

            this.DBContext.Open();

            try
            {
                var session = Session.GetByID(this.dbctx, sessionId);
                if (session == null || !session.IsActive)
                {
                    //删掉无效的 Session
                    Session.Remove(this.dbctx, session.Id);
                    throw new ObjectServer.Exceptions.SecurityException("Not logged!");
                }

                this.dbtx = this.DBContext.BeginTransaction();
                try
                {
                    Session.Pulse(this.dbctx, sessionId);
                    this.resources = Environment.DBProfiles.GetDbProfile(dbName);
                    this.Session = session;
                }
                catch
                {
                    this.DbTransaction.Rollback();
                    this.DbTransaction.Dispose();
                    throw;
                }
            }
            catch
            {
                this.DBContext.Close();
                this.disposed = true;
                throw;
            }

        }

        /// <summary>
        /// 直接建立  context，忽略 session 、登录等
        /// </summary>
        /// <param name="dbName"></param>
        public TransactionContext(string dbName)
            : this(dbName, Environment.DBProfiles.GetDbProfile(dbName))
        {
        }

        public TransactionContext(string dbName, IResourceContainer resourceContainer)
        {
            if (string.IsNullOrEmpty(dbName))
            {
                throw new ArgumentNullException("dbName");
            }
            this.currentThreadID = Thread.CurrentThread.ManagedThreadId;

            this.resources = resourceContainer;
            this.dbctx = DataProvider.CreateDataContext(dbName);
            this.DBContext.Open();

            try
            {
                this.dbtx = this.DBContext.BeginTransaction();
                try
                {
                    this.Session = Session.CreateSystemUserSession();
                    Session.Put(dbctx, this.Session);
                }
                catch
                {
                    this.DbTransaction.Rollback();
                    this.DbTransaction.Dispose();
                    throw;
                }

            }
            catch
            {
                this.DBContext.Close();
                this.disposed = true;
                throw;
            }
        }

        /// <summary>
        /// 构造一个使用 'system' 用户登录的 ServiceScope
        /// </summary>
        /// <param name="db"></param>
        public TransactionContext(IDbContext db)
        {
            if (db == null)
            {
                throw new ArgumentNullException("db");
            }
            this.currentThreadID = Thread.CurrentThread.ManagedThreadId;

            this.resources = Environment.DBProfiles.GetDbProfile(db.DatabaseName);
            this.dbctx = DataProvider.CreateDataContext(db.DatabaseName);
            this.DBContext.Open();

            try
            {
                this.dbtx = this.DBContext.BeginTransaction();
                try
                {
                    this.Session = Session.CreateSystemUserSession();
                    Session.Put(this.dbctx, this.Session);
                }
                catch
                {
                    this.DbTransaction.Rollback();
                    this.DbTransaction.Dispose();
                    throw;
                }
            }
            catch
            {
                this.DBContext.Close();
                this.disposed = true;
                throw;
            }
        }

        ~TransactionContext()
        {
            this.Dispose(false);
        }

        public IResource GetResource(string resName)
        {
            if (string.IsNullOrEmpty(resName))
            {
                throw new ArgumentNullException("resName");
            }

            if (this.disposed)
            {
                throw new ObjectDisposedException("ServiceContext");
            }
            Debug.Assert(this.currentThreadID == Thread.CurrentThread.ManagedThreadId);

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
                throw new ObjectDisposedException("TransactionContext");
            }

            Debug.Assert(this.currentThreadID == Thread.CurrentThread.ManagedThreadId);

            return this.resources.GetResourceDependencyWeight(resName);
        }

        private readonly IDbContext dbctx;
        public IDbContext DBContext
        {
            get
            {
                if (this.disposed)
                {
                    throw new ObjectDisposedException("TransactionContext");
                }

                Debug.Assert(this.dbctx != null);
                Debug.Assert(this.currentThreadID == Thread.CurrentThread.ManagedThreadId);

                return this.dbctx;
            }
        }

        public IResourceContainer Resources
        {
            get
            {
                if (this.disposed)
                {
                    throw new ObjectDisposedException("TransactionContext");
                }

                Debug.Assert(this.resources != null);
                Debug.Assert(this.dbctx != null);
                Debug.Assert(this.currentThreadID == Thread.CurrentThread.ManagedThreadId);

                return this.resources;
            }
        }

        private readonly IDbTransaction dbtx;
        public IDbTransaction DbTransaction
        {
            get
            {
                if (this.disposed)
                {
                    throw new ObjectDisposedException("TransactionContext");
                }

                Debug.Assert(this.dbctx != null);
                Debug.Assert(this.currentThreadID == Thread.CurrentThread.ManagedThreadId);

                return this.dbtx;
            }
        }

        public Session Session { get; private set; }


        #region IDisposable 成员

        private void Dispose(bool isDisposing)
        {
            if (!this.disposed)
            {
                if (isDisposing)
                {
                    //处置托管对象
                }

                try
                {
                    //处置非托管对象
                    if (this.Session.IsSystemUser)
                    {
                        Session.Remove(this.dbctx, this.Session.Id);
                    }

                    this.DbTransaction.Commit();
                }
                catch
                {
                    this.DbTransaction.Rollback();
                }
                finally
                {
                    this.DbTransaction.Dispose();
                    this.DBContext.Close();
                    this.disposed = true;
                }

            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region IEquatable<IContext> 成员

        public bool Equals(ITransactionContext other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            return this.Session.Id == other.Session.Id;
        }

        #endregion

    }
}
