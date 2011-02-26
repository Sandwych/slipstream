using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Reflection;

using ObjectServer.Backend;

namespace ObjectServer
{
    internal class CallingContext : ICallingContext
    {
        private bool ownDb;
        /// <summary>
        /// 安全的创建 CallingContext，会检查 session 等
        /// </summary>
        /// <param name="sessionId"></param>
        public CallingContext(Guid sessionId)
        {
            var sessStore = ObjectServerStarter.SessionStore;
            var session = sessStore.GetSession(sessionId);
            if (session == null || !session.IsActive)
            {
                throw new UnauthorizedAccessException("Not logged!");
            }

            Logger.Info(() =>
                string.Format("CallingContext is opening for sessionId: [{0}]", sessionId));

            this.ownDb = true;
            this.DatabaseContext = ObjectServerStarter.DatabasePool
                .GetPool(session.Database).DatabaseContext;
            this.DatabaseContext.Open();         
        }

        /// <summary>
        /// 直接建立 calling context，忽略 session 、登录等
        /// </summary>
        /// <param name="dbName"></param>
        public CallingContext(string dbName)
        {
            Logger.Info(() =>
                string.Format("CallingContext is opening for database: [{0}]", dbName));

            this.ownDb = true;
            this.Session = new Session(dbName, "system", 0);
            this.DatabaseContext = ObjectServerStarter.DatabasePool
                .GetPool(dbName).DatabaseContext;
            this.DatabaseContext.Open();
        }

        public CallingContext(IDataContext db)
        {
            Logger.Info(() =>
                string.Format("CallingContext is opening for DatabaseContext"));

            this.ownDb = false;
            this.DatabaseContext = db;
            this.DatabaseContext.Open();
            this.Session = new Session("", "system", 0);
        }

        #region ICallingContext 成员

        public IDataContext DatabaseContext
        {
            get;
            private set;
        }

        public Session Session { get; private set; }

        public IObjectPool Pool
        {
            get
            {
                return ObjectServerStarter.DatabasePool.GetPool(this.DatabaseContext.DatabaseName);
            }
        }

        #endregion

        #region IDisposable 成员

        public void Dispose()
        {
            if (this.ownDb)
            {
                this.DatabaseContext.Close();
            }

            Logger.Info(() => "CallingContext closed");
        }

        #endregion
    }
}
