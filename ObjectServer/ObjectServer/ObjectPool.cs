using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace ObjectServer
{
    public sealed class ObjectPool
    {
        private static readonly ObjectPool instance = new ObjectPool();

        private Dictionary<string, IServiceObject> objects =
            new Dictionary<string, IServiceObject>();

        private ObjectPool()
        {
            this.RegisterAllCoreModels();
        }

        private void RegisterAllCoreModels()
        {
            var a = Assembly.GetExecutingAssembly();
            var types = a.GetTypes();
            foreach (var t in types)
            {
                var assemblies = t.GetCustomAttributes(typeof(ModelObjectAttribute), false);
                if (assemblies.Length > 0)
                {
                    var modelObj = (IServiceObject)Activator.CreateInstance(t);
                    this.RegisterModel(modelObj.Name, modelObj);
                }
            }
        }

        public static ObjectPool Instance
        {
            get
            {
                return instance;
            }
        }

        public void RegisterModel(string name, IServiceObject so)
        {
            this.objects.Add(name, so);
        }

        public IServiceObject LookupObject(string name)
        {
            return this.objects[name];
        }
    }
}
