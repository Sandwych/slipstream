using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Transactions;
using System.Data;

using log4net;

using ObjectServer.Module;
using ObjectServer.Backend;

namespace ObjectServer
{
    //TODO: Thread Safty
    public sealed class ObjectPool
    {
        private static readonly ILog Log = LogManager.GetLogger(
            MethodBase.GetCurrentMethod().DeclaringType);

        private Dictionary<string, IServiceObject> objects =
            new Dictionary<string, IServiceObject>();

        public ObjectPool(IDatabase db, string dbName)
        {
            this.Database = dbName;            
            this.RegisterAllCoreModels();

            ObjectServer.Module.Module.LoadModules(db, this);
        }

        public string Database { get; private set; }

        private void RegisterAllCoreModels()
        {
            var a = typeof(ObjectPool).Assembly;
            this.RegisterModelsInAssembly(a);
        }

        public void RegisterModelsInAssembly(Assembly assembly)
        {
            if (Log.IsInfoEnabled)
            {
                Log.InfoFormat("Start to register all models for assembly [{0}]...", assembly.FullName);
            }

            var types = Model.ModelBase.GetModelsFromAssembly(assembly);

            using (var db = Backend.DataProvider.OpenDatabase(this.Database))
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
