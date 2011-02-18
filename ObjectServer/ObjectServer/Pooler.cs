using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Data;
using System.Transactions;

using ObjectServer.Backend;
using ObjectServer.Core;
using ObjectServer.Module;

namespace ObjectServer
{
    /// <summary>
    /// Singleton，TODO 线程安全
    /// </summary>
    public sealed class Pooler
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(
            MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly Pooler s_instance = new Pooler();

        private Dictionary<string, ObjectPool> pools =
            new Dictionary<string, ObjectPool>();

        private Pooler()
        {
            this.RegisterAllDatabases();
        }

        private void RegisterAllDatabases()
        {
            //
            using (var db = new DatabaseBase())
            {
                db.Open();
                var allDbNames = db.List();

                foreach (var dbName in allDbNames)
                {
                    if (Log.IsInfoEnabled)
                    {
                        Log.InfoFormat("Registering object-pool of database: [{0}]", dbName);
                    }
                    var pool = this.RegisterPool(db.Connection, dbName);
                    ModuleModel.LoadModules(db.Connection, dbName, pool);
                }
            }
        }

        private ObjectPool RegisterPool(IDbConnection conn, string dbName)
        {
            using (var tx = new TransactionScope())
            {
                var pool = new ObjectPool(dbName, conn);
                this.pools.Add(dbName.Trim(), pool);
                tx.Complete();
                return pool;
            }
        }

        public static ObjectPool GetPool(string dbName)
        {
            return s_instance.pools[dbName.Trim()];
        }

        public static ICollection<string> ObjectNames
        {
            get { return s_instance.pools.Keys; }
        }

    }
}
