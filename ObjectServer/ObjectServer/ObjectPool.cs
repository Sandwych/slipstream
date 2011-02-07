using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

using log4net;

namespace ObjectServer
{
    public sealed class ObjectPool
    {
        protected static readonly ILog Log = LogManager.GetLogger(
            MethodBase.GetCurrentMethod().DeclaringType);

        private Dictionary<string, IServiceObject> objects =
            new Dictionary<string, IServiceObject>();

        public ObjectPool(string db)
        {
            this.Database = db;
            this.RegisterAllCoreModels();
        }

        public string Database { get; private set; }

        private void RegisterAllCoreModels()
        {
            var a = Assembly.GetExecutingAssembly();
            var types = a.GetTypes();
            foreach (var t in types)
            {
                var assemblies = t.GetCustomAttributes(typeof(ServiceObjectAttribute), false);
                if (assemblies.Length > 0)
                {
                    var obj = Activator.CreateInstance(t) as IServiceObject;
                    if (obj == null)
                    {
                        Log.ErrorFormat("类型 '{0}' 没有实现 IServiceObject 接口", t.FullName);
                    }
                    this.RegisterModel(obj.Name, obj);
                }
            }
        }

        public void RegisterModel(string name, IServiceObject so)
        {
            this.objects[name] = so;
        }

        public IServiceObject LookupObject(string name)
        {
            return this.objects[name];
        }
    }
}
