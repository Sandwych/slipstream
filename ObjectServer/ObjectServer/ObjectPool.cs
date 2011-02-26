using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Transactions;
using System.Data;
using System.Diagnostics;

using ObjectServer.Backend;
using ObjectServer.Model;
using ObjectServer.Utility;

namespace ObjectServer
{

    public sealed class ObjectPool : IObjectPool
    {
        private Dictionary<string, IServiceObject> objects =
            new Dictionary<string, IServiceObject>();

        public ObjectPool(IDataContext dbCtx, string dbName)
        {
            this.DatabaseContext = dbCtx;
            this.DatabaseName = dbName;
            this.LoadAllObjects(dbCtx);

            this.InitializeAllObjects(dbCtx);
        }

        private void LoadAllObjects(IDataContext db)
        {
            Module.RegisterAllCoreObjects(this);

            ObjectServerStarter.ModulePool.UpdateModuleList(db);
            ObjectServerStarter.ModulePool.LoadActivedModules(db, this);
        }

        private void InitializeAllObjects(IDataContext db)
        {
            //一次性初始化所有对象
            //obj.Initialize(db, pool);
            //TODO: 初始化非 IModel 对象
            var objList = this.objects.Values.ToList();
            DependencySort(objList);

            foreach (var m in objList)
            {
                m.Initialize(db, this);
            }
        }

        private static void DependencySort(IList<IServiceObject> objList)
        {
            Debug.Assert(objList != null);

            var objDepends = new Dictionary<string, string[]>();
            foreach (var obj in objList)
            {
                objDepends.Add(obj.Name, obj.GetReferencedObjects());
            }
            objList.DependencySort(m => m.Name, m => objDepends[m.Name]);
        }

        public string DatabaseName { get; private set; }

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

        public IDataContext DatabaseContext { get; private set; }


        #region ServiceObject(s) factory methods

        internal static IServiceObject CreateStaticObjectInstance(Type t)
        {
            var obj = Activator.CreateInstance(t) as IServiceObject;
            if (obj == null)
            {
                var msg = string.Format("类型 '{0}' 没有实现 IServiceObject 接口", t.FullName);
                throw new InvalidCastException(msg);
            }
            return obj;
        }

        internal static T CreateStaticObjectInstance<T>()
            where T : class, IServiceObject
        {
            return CreateStaticObjectInstance(typeof(T)) as T;
        }

        //以后要支持 DLR，增加  CreateDynamicObjectInstance

        #endregion


        #region IDisposable 成员

        public void Dispose()
        {
            this.DatabaseContext.Dispose();
        }

        #endregion
    }
}
