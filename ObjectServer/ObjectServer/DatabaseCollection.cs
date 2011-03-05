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
    internal sealed class DatabaseCollection : IGlobalObject, IDisposable
    {
        private Config config;

        private Dictionary<string, Database> databases =
            new Dictionary<string, Database>();


        ~DatabaseCollection()
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

            var db = new Database(dbName);

            lock (this)
            {
                this.databases.Add(dbName.Trim(), db);
            }

            this.LoadAdditionalModules(session, db);
        }

        private void LoadAdditionalModules(Session session, Database db)
        {
            //加载其它模块
            Logger.Info(() => "Loading additional modules...");
            var ctx = new SystemContext(db, session);
            ObjectServerStarter.Modules.UpdateModuleList(db.DataContext);
            ObjectServerStarter.Modules.LoadActivatedModules(ctx);
        }

        internal Database GetDatabase(Session session)
        {
            if (!this.databases.ContainsKey(session.Database))
            {
                this.LoadDatabase(session);
            }

            return this.databases[session.Database];
        }

        internal void RemoveDatabase(string dbName)
        {
            //TODO: 这里要处理并发
            //比如两个客户端，一个正在操作数据库，另一个要删除数据库

            lock (this)
            {
                var db = this.databases[dbName];
                db.DataContext.Close();
                this.databases.Remove(dbName);
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

            foreach (var p in this.databases)
            {
                p.Value.Dispose();
            }
        }

        #endregion
    }
}
