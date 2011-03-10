using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Data;
using System.Transactions;
using System.Runtime.CompilerServices;

using ObjectServer.Backend;
using ObjectServer.Core;

namespace ObjectServer
{
    /// <summary>
    /// Singleton
    /// </summary>
    internal sealed class DatabaseProfileCollection : IGlobalObject, IDisposable
    {
        private Config config;

        private Dictionary<string, DatabaseProfile> databaseProfiles =
            new Dictionary<string, DatabaseProfile>();


        ~DatabaseProfileCollection()
        {
            this.Dispose(false);
        }

        #region IGlobalObject 成员

        public void Initialize(Config cfg)
        {
            this.config = cfg;
        }

        #endregion

        internal void LoadDatabase(Session session)
        {
            var dbName = session.Database;
            Logger.Info(() => string.Format("Registering object-pool of database: [{0}]", dbName));

            var dbNames = DataProvider.ListDatabases();
            if (!dbNames.Contains(dbName))
            {
                throw new DatabaseNotFoundException("Cannot found database: " + dbName, dbName);
            }

            var db = new DatabaseProfile(dbName);

            lock (this)
            {
                this.databaseProfiles.Add(dbName.Trim(), db);
            }

            this.LoadAdditionalModules(session, db);
        }

        private void LoadAdditionalModules(Session session, DatabaseProfile db)
        {
            //加载其它模块
            Logger.Info(() => "Loading additional modules...");
            var ctx = new InternalResourceScope(db, session);
            ObjectServerStarter.Modules.UpdateModuleList(db.DataContext);
            ObjectServerStarter.Modules.LoadActivatedModules(ctx);
        }

        internal DatabaseProfile GetDatabase(Session session)
        {
            if (!this.databaseProfiles.ContainsKey(session.Database))
            {
                this.LoadDatabase(session);
            }

            return this.databaseProfiles[session.Database];
        }

        internal void RemoveDatabase(string dbName)
        {
            //TODO: 这里要处理并发
            //比如两个客户端，一个正在操作数据库，另一个要删除数据库

            lock (this)
            {
                var db = this.databaseProfiles[dbName];
                db.DataContext.Close();
                this.databaseProfiles.Remove(dbName);
            }
        }

        #region IDisposable 成员

        public void Dispose()
        {
            this.Dispose(true);
        }

        public void Dispose(bool disposing)
        {
            if (disposing)
            {
            }

            foreach (var p in this.databaseProfiles)
            {
                p.Value.Dispose();
            }
        }

        #endregion
    }
}
