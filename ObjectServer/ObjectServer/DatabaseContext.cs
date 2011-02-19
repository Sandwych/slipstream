using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Data;
using System.Transactions;

using ObjectServer.Backend;
using ObjectServer.Core;

namespace ObjectServer
{
    /// <summary>
    /// Singleton，TODO 线程安全
    /// </summary>
    internal sealed class DatabaseContext
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(
            MethodBase.GetCurrentMethod().DeclaringType);

        private Dictionary<string, ObjectPool> pools =
            new Dictionary<string, ObjectPool>();

        private static readonly object _poolLock = new object();


        internal void RegisterDatabase(string dbName)
        {
            using (var db = DataProvider.OpenDatabase(dbName))
            {
                RegisterDatabase(db, dbName);
            }
        }

        internal void RegisterDatabase(IDatabase db, string dbName)
        {
            if (Log.IsInfoEnabled)
            {
                Log.InfoFormat("Registering object-pool of database: [{0}]", dbName);
            }

            var dbNames = db.List();
            if (!dbNames.Contains(dbName))
            {
                throw new ArgumentException("dbName");
            }

            var pool = new ObjectPool(db, dbName);

            lock (_poolLock)
            {
                this.pools.Add(dbName.Trim(), pool);
            }
        }

        internal ObjectPool GetPool(string dbName)
        {
            lock (_poolLock)
            {
                if (!this.pools.ContainsKey(dbName))
                {
                    RegisterDatabase(dbName);
                }

                return this.pools[dbName.Trim()];
            }
        }

    }
}
