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
            this.Database = DataProvider.OpenDatabase(session.Database);

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
            this.Database = DataProvider.OpenDatabase(dbName);
            this.Session = new Session(dbName, "system", 0);
        }

        public CallingContext(IDatabaseContext db)
        {
            Logger.Info(() =>
                string.Format("CallingContext is opening for DatabaseContext"));

            this.ownDb = false;
            this.Database = db;
            this.Session = new Session("", "system", 0);
        }

        #region ICallingContext 成员

        public IDatabaseContext Database
        {
            get;
            private set;
        }

        public Session Session { get; private set; }

        public ObjectPool Pool
        {
            get
            {
                return ObjectServerStarter.Pooler.GetPool(this.Database.DatabaseName);
            }
        }

        #endregion

        #region IDisposable 成员

        public void Dispose()
        {
            if (this.ownDb)
            {
                this.Database.Close();
            }

            Logger.Info(() => "CallingContext closed");
        }

        #endregion
    }
}
