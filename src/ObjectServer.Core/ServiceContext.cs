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
    /// 但凡是需要 RPC 的方法都需要用此 scope 包裹
    /// </summary>
    internal sealed class ServiceContext : IServiceContext
    {
        private bool disposed = false;
        private readonly IResourceContainer resources;

        /// <summary>
        /// 安全的创建 Context，会检查 session 等
        /// </summary>
        /// <param name="sessionId"></param>
        public ServiceContext(string sessionId)
        {
            var sessStore = Environment.SessionStore;
            var session = sessStore.GetSession(sessionId);
            if (session == null || !session.IsActive)
            {
                throw new ObjectServer.Exceptions.SecurityException("Not logged!");
            }

            LoggerProvider.EnvironmentLogger.Debug(() =>
                string.Format("ContextScope is opening for sessionId: [{0}]", sessionId));

            this.resources = Environment.DBProfiles.GetDBProfile(session.DBName);

            this.Session = session;
            this.SessionStore.Pulse(session.ID);

            this.dbctx = DataProvider.CreateDataContext(session.DBName);
            this.DBContext.Open();
        }

        /// <summary>
        /// 直接建立  context，忽略 session 、登录等
        /// </summary>
        /// <param name="dbName"></param>
        public ServiceContext(string dbName, string login)
        {
            LoggerProvider.EnvironmentLogger.Debug(() =>
                string.Format("ContextScope is opening for database: [{0}]", dbName));

            this.resources = Environment.DBProfiles.GetDBProfile(dbName);
            this.Session = new Session(dbName);
            this.SessionStore.PutSession(this.Session);
            this.dbctx = DataProvider.CreateDataContext(dbName);
            this.DBContext.Open();
        }

        /// <summary>
        /// 构造一个使用 'system' 用户登录的 ServiceScope
        /// </summary>
        /// <param name="db"></param>
        public ServiceContext(IDBContext db)
        {
            Debug.Assert(db != null);

            LoggerProvider.EnvironmentLogger.Debug(() =>
                string.Format("ContextScope is opening for DatabaseContext"));

            this.resources = Environment.DBProfiles.GetDBProfile(db.DatabaseName);
            this.Session = new Session(db.DatabaseName);
            this.SessionStore.PutSession(this.Session);
            this.dbctx = DataProvider.CreateDataContext(db.DatabaseName);
            this.DBContext.Open();
        }

        ~ServiceContext()
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
                this.DBContext.Close();

                this.disposed = true;
                LoggerProvider.EnvironmentLogger.Debug(() => "ScopeContext closed");
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region IEquatable<IContext> 成员

        public bool Equals(IServiceContext other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            return this.Session.ID == other.Session.ID;
        }

        #endregion

        public override int GetHashCode()
        {
            return this.Session.ID.GetHashCode();
        }

        private SessionStore SessionStore
        {
            get { return Environment.SessionStore; }
        }
    }
}
