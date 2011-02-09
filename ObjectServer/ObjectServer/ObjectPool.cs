using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Transactions;
using System.Data;

using log4net;

namespace ObjectServer
{
    public sealed class ObjectPool
    {
        private static readonly ILog Log = LogManager.GetLogger(
            MethodBase.GetCurrentMethod().DeclaringType);

        private Dictionary<string, IServiceObject> objects =
            new Dictionary<string, IServiceObject>();

        public ObjectPool(string db, IDbConnection conn)
        {
            this.Database = db;
            this.RegisterAllCoreModels(conn);
        }

        public string Database { get; private set; }

        private void RegisterAllCoreModels(IDbConnection conn)
        {
            if (Log.IsInfoEnabled)
            {
                Log.InfoFormat("Start to register all core models...");
            }
            var types = Model.ModelBase.GetAllCoreModels();

            using (var db = new Backend.Database(this.Database))
            {
                db.Open();
                foreach (var t in types)
                {
                    var obj = Model.ModelBase.CreateModelInstance(db, t);
                    this.RegisterServiceObject(obj.Name, obj);
                }
            }

            if (Log.IsInfoEnabled)
            {
                Log.InfoFormat("Done");
            }
        }

        public void RegisterServiceObject(string name, IServiceObject so)
        {
            if (Log.IsInfoEnabled)
            {
                Log.InfoFormat("Register model: {0}", name);
            }

            this.objects[name] = so;
        }

        public IServiceObject LookupObject(string name)
        {
            return this.objects[name];
        }
    }
}
