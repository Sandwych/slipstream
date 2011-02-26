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
    internal sealed class DatabasePool : IGlobalObject, IDisposable
    {
        private Dictionary<string, IObjectPool> pools =
            new Dictionary<string, IObjectPool>();


        #region IGlobalObject 成员

        public void Initialize(Config cfg)
        {
        }

        #endregion


        [MethodImpl(MethodImplOptions.Synchronized)]
        internal void RegisterDatabase(string dbName)
        {
            var db = DataProvider.CreateDataContext(dbName);
            RegisterDatabase(db, dbName);
        }


        [MethodImpl(MethodImplOptions.Synchronized)]
        internal void RegisterDatabase(IDataContext db, string dbName)
        {
            Logger.Info(() => string.Format("Registering object-pool of database: [{0}]", dbName));

            var dbNames = DataProvider.ListDatabases();
            if (!dbNames.Contains(dbName))
            {
                throw new ArgumentException("Invalid database name", "dbName");
            }

            var pool = new ObjectPool(db, dbName);

            this.pools.Add(dbName.Trim(), pool);
        }

        internal IObjectPool GetPool(string dbName)
        {
            if (!this.pools.ContainsKey(dbName))
            {
                RegisterDatabase(dbName);
            }

            return this.pools[dbName.Trim()];
        }


        #region IDisposable 成员

        public void Dispose()
        {
            foreach (var p in this.pools)
            {
                p.Value.Dispose();
            }
        }

        #endregion
    }
}
