using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Data;
using System.Transactions;
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
    internal sealed class DbProfileCollection : IGlobalObject, IDisposable
    {
        private Config config;
        private Dictionary<string, IDbProfile> dbProfiles =
            new Dictionary<string, IDbProfile>();
        private bool disposed = false;

        ~DbProfileCollection()
        {
            this.Dispose(false);
        }

        #region IGlobalObject 成员

        public void Initialize(Config cfg)
        {
            Debug.Assert(cfg != null);

            this.config = cfg;

            LoggerProvider.EnvironmentLogger.Info("DbProfileCollection has been loaded.");
        }

        #endregion

        public void Register(string dbName, bool isUpdate)
        {
            if (string.IsNullOrEmpty(dbName))
            {
                throw new ArgumentNullException("dbName");
            }

            var msg = String.Format("Loading database profile: [{0}]", dbName);
            LoggerProvider.EnvironmentLogger.Info(msg);

            if (this.dbProfiles.ContainsKey(dbName))
            {
                throw new ArgumentException("dbName");
            }

            var dbNames = DataProvider.ListDatabases();
            if (!dbNames.Contains(dbName))
            {
                throw new DatabaseNotFoundException("Cannot found database: " + dbName, dbName);
            }

            var db = new DbProfile(dbName);
            db.Initialize(isUpdate);
            this.dbProfiles.Add(dbName, db);
        }

        public IDbProfile GetDbProfile(string dbName)
        {
            Debug.Assert(!string.IsNullOrEmpty(dbName));

            if (!this.dbProfiles.ContainsKey(dbName))
            {
                this.Register(dbName, false);
            }

            return this.dbProfiles[dbName];
        }

        public void Remove(string dbName)
        {
            Debug.Assert(!string.IsNullOrEmpty(dbName));

            //比如两个客户端，一个正在操作数据库，另一个要删除数据库
            //TODO 线程安全

            if (!this.dbProfiles.ContainsKey(dbName))
            {
                throw new DatabaseNotFoundException("Cannot found database: " + dbName, dbName);
            }

            var db = this.dbProfiles[dbName];
            this.dbProfiles.Remove(dbName);
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
