using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Data;
using System.Transactions;

using ObjectServer.Backend;

namespace ObjectServer
{
    /// <summary>
    /// Singleton
    /// </summary>
    public sealed class Pooler
    {
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
            using (var tx = new TransactionScope())
            {
                db.Open();
                var allDbNames = db.List();

                foreach (var dbName in allDbNames)
                {
                    this.RegisterPool(db.Connection, dbName);
                }

                tx.Complete();
            }
        }

        private void RegisterPool(IDbConnection conn, string dbName)
        {
            var pool = new ObjectPool(dbName, conn);
            this.pools.Add(dbName, pool);
        }

        public ObjectPool GetPool(string dbName)
        {
            return this.pools[dbName];
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
