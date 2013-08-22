using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Data;
using System.Transactions;
using System.Diagnostics;
using System.Threading;

using SlipStream.Exceptions;
using SlipStream.Data;
using SlipStream.Core;

namespace SlipStream
{
    /// <summary>
    /// Singleton
    /// </summary>
    internal sealed class DbDomainManager : IDbDomainManager
    {
        private ShellSettings _shellSettings;
        private Dictionary<string, IDbDomain> dbProfiles =
            new Dictionary<string, IDbDomain>();
        private bool disposed = false;
        private readonly IDataProvider _dataProvider;
        private readonly IModuleManager _modules;

        public DbDomainManager(IDataProvider dataProvider, IModuleManager modules, ShellSettings settings)
        {
            LoggerProvider.EnvironmentLogger.Info(() => "Initializing databases...");
            this._dataProvider = dataProvider;
            this._modules = modules;
            this._shellSettings = settings;
        }

        ~DbDomainManager()
        {
            this.Dispose(false);
        }

        public void Register(string dbName, bool isUpdate)
        {
            if (string.IsNullOrEmpty(dbName))
            {
                throw new ArgumentNullException("_dbName");
            }

            var msg = String.Format("Loading database profile: [{0}]", dbName);
            LoggerProvider.EnvironmentLogger.Info(msg);

            if (this.dbProfiles.ContainsKey(dbName))
            {
                throw new ArgumentException("_dbName");
            }

            var dbNames = this._dataProvider.ListDatabases();
            if (!dbNames.Contains(dbName))
            {
                throw new DatabaseNotFoundException("Cannot found database: " + dbName, dbName);
            }

            var db = new DbDomain(this._dataProvider, this._modules, dbName);
            db.Initialize(isUpdate);
            this.dbProfiles.Add(dbName, db);
        }

        public IDbDomain GetDbDomain(string dbName)
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

            if (this.dbProfiles.ContainsKey(dbName))
            {
                var db = this.dbProfiles[dbName];
                this.dbProfiles.Remove(dbName);
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
