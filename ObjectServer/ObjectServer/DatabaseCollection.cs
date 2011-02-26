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

        internal void LoadDatabase(string dbName)
        {
            Logger.Info(() => string.Format("Registering object-pool of database: [{0}]", dbName));

            var dbNames = DataProvider.ListDatabases();
            if (!dbNames.Contains(dbName))
            {
                throw new ArgumentException("Invalid database name", "dbName");
            }

            var db = new Database(dbName);
            db.Initialize(this.config);

            lock (this)
            {
                this.databases.Add(dbName.Trim(), db);
            }
        }

        internal Database GetDatabase(string dbName)
        {
            if (!this.databases.ContainsKey(dbName))
            {
                LoadDatabase(dbName);
            }

            return this.databases[dbName];
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
