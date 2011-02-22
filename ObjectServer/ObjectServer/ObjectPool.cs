using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Transactions;
using System.Data;

using ObjectServer.Backend;
using ObjectServer.Model;
using ObjectServer.Utility;

namespace ObjectServer
{

    public sealed class ObjectPool
    {
        private Dictionary<string, IServiceObject> objects =
            new Dictionary<string, IServiceObject>();

        public ObjectPool(IDatabaseContext db, string dbName)
        {
            this.Database = dbName;
            this.LoadAllObjects(db);

            this.InitializeAllObjects(db);
        }

        private void LoadAllObjects(IDatabaseContext db)
        {
            this.RegisterAllCoreObjects();

            ObjectServerStarter.ModulePool.LoadActivedModules(db, this);
        }

        private void InitializeAllObjects(IDatabaseContext db)
        {
            //一次性初始化所有对象
            //obj.Initialize(db, pool);
            //TODO: 初始化非 IModel 对象
            var objList = this.objects.Values.ToList();
            objList.DependencySort(m => m.Name, m => m.ReferencedObjects);

            foreach (var m in objList)
            {
                m.Initialize(db, this);
            }
        }

        public string Database { get; private set; }

        public void AddModelsWithinAssembly(Assembly assembly)
        {
            Logger.Info(() => string.Format(
                "Start to register all models for assembly [{0}]...", assembly.FullName));

            var types = GetStaticModelsFromAssembly(assembly);

            using (var db = Backend.DataProvider.OpenDatabase(this.Database))
            {
                db.Open();
                foreach (var t in types)
                {
                    var obj = ObjectPool.CreateStaticObjectInstance(t);
                    this.objects[obj.Name] = obj;
                }
            }
        }

        public void AddServiceObject(IServiceObject obj)
        {
            //TODO 处理对象已经存在的问题，继承等

            this.objects.Add(obj.Name, obj);
        }

        public IServiceObject this[string objName]
        {
            get
            {
                IServiceObject so;
                if (!this.objects.TryGetValue(objName, out so))
                {
                    var msg = string.Format("Cannot found service object: {0} ", objName);
                    throw new ServiceObjectNotFoundException(msg, objName);
                }
                return this.objects[objName];
            }
        }

        /// <summary>
        /// 检查是否包含对象 objName
        /// </summary>
        /// <param name="objName"></param>
        /// <returns></returns>
        public bool Contains(string objName)
        {
            return this.objects.ContainsKey(objName);
        }


        private void RegisterAllCoreObjects()
        {
            var a = typeof(ObjectPool).Assembly;
            this.AddModelsWithinAssembly(a);
        }


        private static Type[] GetStaticModelsFromAssembly(Assembly assembly)
        {
            var types = assembly.GetTypes();
            var result = new List<Type>();
            foreach (var t in types)
            {
                var assemblies = t.GetCustomAttributes(typeof(ServiceObjectAttribute), false);
                if (assemblies.Length > 0)
                {
                    result.Add(t);
                }
            }
            return result.ToArray();
        }

        #region ServiceObject(s) factory methods

        public static IServiceObject CreateStaticObjectInstance(Type t)
        {
            var obj = Activator.CreateInstance(t) as IServiceObject;
            if (obj == null)
            {
                var msg = string.Format("类型 '{0}' 没有实现 IServiceObject 接口", t.FullName);
                throw new InvalidCastException(msg);
            }
            return obj;
        }

        public static T CreateStaticObjectInstance<T>()
            where T : class, IServiceObject
        {
            return CreateStaticObjectInstance(typeof(T)) as T;
        }

        //以后要支持 DLR，增加  CreateDynamicObjectInstance

        #endregion

    }
}
