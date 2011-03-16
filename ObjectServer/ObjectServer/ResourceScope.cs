using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Reflection;
using System.Diagnostics;

using ObjectServer.Backend;

namespace ObjectServer
{
    /// <summary>
    /// 但凡是需要 RPC 的方法都需要用此 scope 包裹
    /// </summary>
    public sealed class ResourceScope : IResourceScope
    {
        private bool ownDb;
        /// <summary>
        /// 安全的创建 Context，会检查 session 等
        /// </summary>
        /// <param name="sessionId"></param>
        public ResourceScope(Guid sessionId)
        {
            var sessStore = ObjectServerStarter.SessionStore;
            var session = sessStore.GetSession(sessionId);
            if (session == null || !session.IsActive)
            {
                throw new UnauthorizedAccessException("Not logged!");
            }

            Logger.Debug(() =>
                string.Format("ContextScope is opening for sessionId: [{0}]", sessionId));

            this.Session = session;
            this.SessionStore.Pulse(session.Id);

            this.ownDb = true;
            this.DatabaseProfile = ObjectServerStarter.DatabaseProfiles.GetDatabase(session);
            this.DatabaseProfile.DataContext.Open();
        }

        /// <summary>
        /// 直接建立  context，忽略 session 、登录等
        /// </summary>
        /// <param name="dbName"></param>
        internal ResourceScope(string dbName)
        {
            Logger.Debug(() =>
                string.Format("ContextScope is opening for database: [{0}]", dbName));

            this.Session = new Session(dbName, "system", 0);
            this.SessionStore.PutSession(this.Session);
            this.ownDb = true;
            this.DatabaseProfile = ObjectServerStarter.DatabaseProfiles.GetDatabase(this.Session);
            this.DatabaseProfile.DataContext.Open();
        }

        internal ResourceScope(IDatabaseProfile db)
        {
            Debug.Assert(db != null);
            Debug.Assert(db.DataContext != null);

            Logger.Debug(() =>
                string.Format("ContextScope is opening for DatabaseContext"));

            this.Session = new Session(db.DataContext.DatabaseName, "system", 0);
            this.SessionStore.PutSession(this.Session);
            this.ownDb = false;
            this.DatabaseProfile = db;
            this.DatabaseProfile.DataContext.Open();
        }

        #region IContext 成员

        public IDatabaseProfile DatabaseProfile
        {
            get;
            private set;
        }

        public Session Session { get; private set; }

        #endregion

        #region IDisposable 成员

        public void Dispose()
        {
            if (this.ownDb)
            {
                this.DatabaseProfile.DataContext.Close();
            }

            Logger.Debug(() => "ScopeContext closed");
        }

        #endregion

        #region IEquatable<IContext> 成员

        public bool Equals(IResourceScope other)
        {
            return this.Session.Id == other.Session.Id;
        }

        #endregion

        public override int GetHashCode()
        {
            return this.Session.Id.GetHashCode();
        }

        private SessionStore SessionStore
        {
            get { return ObjectServerStarter.SessionStore; }
        }
    }
}
