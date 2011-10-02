using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Data;
using System.Transactions;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.Threading;

using ObjectServer.Exceptions;
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
        private bool disposed = false;
        private readonly ReaderWriterLockSlim dbProfilesLock = new ReaderWriterLockSlim();


        ~DBProfileCollection()
        {
            this.Dispose(false);
        }

        #region IGlobalObject 成员

        public void Initialize(Config cfg)
        {
            Debug.Assert(cfg != null);

            this.config = cfg;

            var dbs = DataProvider.ListDatabases();
            foreach (var dbName in dbs)
            {
                bool isRegistered;

                this.dbProfilesLock.EnterReadLock();
                try
                {
                    isRegistered = this.dbProfiles.ContainsKey(dbName);
                }
                finally
                {
                    this.dbProfilesLock.ExitReadLock();
                }

                if (isRegistered)
                {
                    LoadDB(dbName);
                }
            }

            LoggerProvider.EnvironmentLogger.Info("All databases has been loaded.");
        }

        #endregion

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void LoadDB(string dbName)
        {
            Debug.Assert(!string.IsNullOrEmpty(dbName));

            var msg = String.Format("Initializing DBProfile: [{0}]".PadRight(80, '='), dbName);
            LoggerProvider.EnvironmentLogger.Info(msg);

            LoggerProvider.EnvironmentLogger.Info(() => string.Format("Registering resources in database: [{0}]", dbName));

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

            using (var dbctx = DataProvider.CreateDataContext(dbName))
            {
                this.LoadModules(dbctx);
            }
        }

        private void LoadModules(IDBContext db)
        {
            Debug.Assert(db != null);

            //加载其它模块
            LoggerProvider.EnvironmentLogger.Info(() => "Loading modules...");
            using (var ctx = new SystemTransactionContext(db))
            {
                Environment.Modules.UpdateModuleList(db);
                Environment.Modules.LoadModules(ctx);
            }
        }

        public DBProfile GetDBProfile(string dbName)
        {
            Debug.Assert(!string.IsNullOrEmpty(dbName));

            this.dbProfilesLock.EnterReadLock();
            try
            {
                if (!this.dbProfiles.ContainsKey(dbName))
                {
                    throw new DatabaseNotFoundException("Cannot found database: " + dbName, dbName);
                }

                return this.dbProfiles[dbName];
            }
            finally
            {
                this.dbProfilesLock.ExitReadLock();
            }
        }

        public void RemoveDB(string dbName)
        {
            Debug.Assert(!string.IsNullOrEmpty(dbName));

            //比如两个客户端，一个正在操作数据库，另一个要删除数据库
            this.dbProfilesLock.EnterWriteLock();
            try
            {

                var db = this.dbProfiles[dbName];
                this.dbProfiles.Remove(dbName);
            }
            finally
            {
                this.dbProfilesLock.ExitWriteLock();
            }
        }

        #region IDisposable 成员

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            this.Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    //处置托管对象
                }

                //处置非托管对象
                foreach (var p in this.dbProfiles)
                {
                    p.Value.Dispose();
                }

                this.disposed = true;
            }
        }

        #endregion
    }
}
