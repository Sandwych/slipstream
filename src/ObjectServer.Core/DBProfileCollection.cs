using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Data;
using System.Transactions;
using System.Runtime.CompilerServices;
using System.Diagnostics;

using ObjectServer.Data;
using ObjectServer.Core;

namespace ObjectServer
{
    /// <summary>
    /// Singleton
    /// </summary>
    internal sealed class DBProfileCollection : IGlobalObject, IDisposable
    {
        private Config config;
        private Dictionary<string, DBProfile> dbProfiles =
            new Dictionary<string, DBProfile>();


        ~DBProfileCollection()
        {
            this.Dispose(false);
        }

        #region IGlobalObject 成员

        public void Initialize(Config cfg)
        {
            Debug.Assert(cfg != null);

            this.config = cfg;
        }

        #endregion

        [MethodImpl(MethodImplOptions.Synchronized)]
        internal void LoadDatabase(Session session)
        {
            Debug.Assert(session != null);

            var dbName = session.Database;
            Logger.Info(() => string.Format("Registering object-pool of database: [{0}]", dbName));

            var dbNames = DataProvider.ListDatabases();
            if (!dbNames.Contains(dbName))
            {
                throw new DatabaseNotFoundException("Cannot found database: " + dbName, dbName);
            }

            var db = new DBProfile(dbName);

            lock (this)
            {
                this.dbProfiles.Add(dbName.Trim(), db);
            }

            this.LoadAdditionalModules(session, db);
        }

        private void LoadAdditionalModules(Session session, DBProfile db)
        {
            Debug.Assert(session != null);
            Debug.Assert(db != null);

            //加载其它模块
            Logger.Info(() => "Loading additional modules...");
            using (var ctx = new InternalServiceScope(db, session))
            {
                Platform.Modules.UpdateModuleList(db.Connection);
                Platform.Modules.LoadActivatedModules(ctx);
            }
        }

        public DBProfile TryGetDBProfile(Session session)
        {
            Debug.Assert(session != null);

            if (!this.dbProfiles.ContainsKey(session.Database))
            {
                this.LoadDatabase(session);
            }

            return this.dbProfiles[session.Database];
        }

        public DBProfile GetDBProfile(string dbName)
        {
            Debug.Assert(!string.IsNullOrEmpty(dbName));
            if (!this.dbProfiles.ContainsKey(dbName))
            {
                throw new DatabaseNotFoundException("Cannot found database: " + dbName, dbName);
            }

            return this.dbProfiles[dbName];
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void RemoveDB(string dbName)
        {
            Debug.Assert(!string.IsNullOrEmpty(dbName));

            //比如两个客户端，一个正在操作数据库，另一个要删除数据库

            var db = this.dbProfiles[dbName];
            db.Connection.Close();
            this.dbProfiles.Remove(dbName);
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

            foreach (var p in this.dbProfiles)
            {
                p.Value.Dispose();
            }
        }

        #endregion
    }
}
