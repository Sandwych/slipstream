using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Reflection;
using System.Diagnostics;

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

            this.dbctx = DataProvider.CreateDataContext(dbName);
            this.DBContext.Open();

            var session = Session.GetByID(this.dbctx, sessionId);
            Session.Pulse(this.dbctx, sessionId);
            if (session == null || !session.IsActive)
            {
                throw new ObjectServer.Exceptions.SecurityException("Not logged!");
            }

            this.resources = Environment.DBProfiles.GetDBProfile(dbName);

            this.Session = session;
        }

        /// <summary>
        /// 直接建立  context，忽略 session 、登录等
        /// </summary>
        /// <param name="dbName"></param>
        public TransactionContext(string dbName)
        {
            if (string.IsNullOrEmpty(dbName))
            {
                throw new ArgumentNullException("dbName");
            }

            this.resources = Environment.DBProfiles.GetDBProfile(dbName);
            this.dbctx = DataProvider.CreateDataContext(dbName);
            this.DBContext.Open();
            this.Session = Session.CreateSystemUserSession();
            Session.Put(dbctx, this.Session);
        }

        /// <summary>
        /// 构造一个使用 'system' 用户登录的 ServiceScope
        /// </summary>
        /// <param name="db"></param>
        public TransactionContext(IDBContext db)
        {
            if (db == null)
            {
                throw new ArgumentNullException("db");
            }

            this.resources = Environment.DBProfiles.GetDBProfile(db.DatabaseName);
            this.dbctx = DataProvider.CreateDataContext(db.DatabaseName);
            this.DBContext.Open();
            this.Session = Session.CreateSystemUserSession();
            Session.Put(this.dbctx, this.Session);
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
                throw new ObjectDisposedException("ServiceContext");
            }

            return this.resources.GetResourceDependencyWeight(resName);
        }

        private IDBContext dbctx;
        public IDBContext DBContext
        {
            get
            {
                if (this.disposed)
                {
                    throw new ObjectDisposedException("ServiceContext");
                }

                Debug.Assert(this.dbctx != null);

                return this.dbctx;
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

                //处置非托管对象
                if (this.Session.IsSystemUser)
                {
                    Session.Remove(this.dbctx, this.Session.ID);
                }

                this.DBContext.Close();

                this.disposed = true;
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

            return this.Session.ID == other.Session.ID;
        }

        #endregion

    }
}
