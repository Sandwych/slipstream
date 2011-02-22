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
    internal sealed class Database
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(
            MethodBase.GetCurrentMethod().DeclaringType);

        private Dictionary<string, ObjectPool> pools =
            new Dictionary<string, ObjectPool>();


        [MethodImpl(MethodImplOptions.Synchronized)]
        internal void RegisterDatabase(string dbName)
        {
            using (var db = DataProvider.OpenDatabase(dbName))
            {
                RegisterDatabase(db, dbName);
            }
        }


        [MethodImpl(MethodImplOptions.Synchronized)]
        internal void RegisterDatabase(IDatabaseContext db, string dbName)
        {
            if (Log.IsInfoEnabled)
            {
                Log.InfoFormat("Registering object-pool of database: [{0}]", dbName);
            }

            var dbNames = db.List();
            if (!dbNames.Contains(dbName))
            {
                throw new ArgumentException("Invalid database name", "dbName");
            }

            var pool = new ObjectPool(db, dbName);

            this.pools.Add(dbName.Trim(), pool);
        }

        internal ObjectPool GetPool(string dbName)
        {
            if (!this.pools.ContainsKey(dbName))
            {
                RegisterDatabase(dbName);
            }

            return this.pools[dbName.Trim()];
        }

    }
}
