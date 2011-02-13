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
    /// Singleton
    /// </summary>
    public sealed class Pooler
    {
        protected static readonly log4net.ILog Log = log4net.LogManager.GetLogger(
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
            using (var db = new Database())
            {
                db.Open();
                var allDbNames = db.List();

                foreach (var dbName in allDbNames)
                {
                    if (Log.IsInfoEnabled)
                    {
                        Log.InfoFormat("Registering object-pool of database: [{0}]", dbName);
                    }
                    this.RegisterPool(db.Connection, dbName);
                    ObjectServer.Core.Module.LoadModules(dbName, db.Connection);
                }
            }
        }

        private void RegisterPool(IDbConnection conn, string dbName)
        {
            using (var tx = new TransactionScope())
            {
                var pool = new ObjectPool(dbName, conn);
                this.pools.Add(dbName.Trim(), pool);
                tx.Complete();
            }
        }

        public ObjectPool GetPool(string dbName)
        {
            return this.pools[dbName.Trim()];
        }

        public ICollection<string> ObjectNames
        {
            get { return this.pools.Keys; }
        }

        public static Pooler Instance
        {
            get
            {
                return s_instance;
            }
        }
    }
}
