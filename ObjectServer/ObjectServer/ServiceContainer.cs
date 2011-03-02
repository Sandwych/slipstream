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

    public sealed class ServiceContainer : IServiceContainer
    {
        private IDictionary<string, IObjectService> objects =
            new Dictionary<string, IObjectService>();

        private IDatabase database;
        private bool initialized;

        public ServiceContainer(IDatabase db)
        {
            this.database = db;
            this.initialized = false;
        }

        #region IServiceContainer 成员

        public void Initialize()
        {
            Debug.Assert(!this.initialized);

            lock (this)
            {
                this.LoadAllObjects(this.database);
                this.InitializeAllObjects(this.database);

                this.initialized = true;
            }
        }

        public void RegisterObject(IObjectService so)
        {
            Debug.Assert(!this.initialized);

            lock (this)
            {
                this.objects.Add(so.Name, so);
            }
        }


        public IObjectService Resolve(string objName)
        {
            Debug.Assert(this.initialized);

            if (!this.objects.ContainsKey(objName))
            {
                var msg = string.Format("Cannot found service object: '{0}'", objName);
                Logger.Error(() => msg);

                throw new ServiceObjectNotFoundException(msg, objName);
            }

            return this.objects[objName];
        }

        #endregion

        private void LoadAllObjects(IDatabase db)
        {
            Debug.Assert(db != null);
            Debug.Assert(!this.initialized);

            this.objects.Clear();
            Module.RegisterAllCoreObjects(this);

            ObjectServerStarter.Modules.UpdateModuleList(db.DataContext);
            ObjectServerStarter.Modules.LoadActivatedModules(db.DataContext, this);
        }

        private void InitializeAllObjects(IDatabase db)
        {
            Debug.Assert(db != null);
            Debug.Assert(!this.initialized);

            //一次性初始化所有对象
            //obj.Initialize(db, pool);
            //TODO: 初始化非 IModel 对象
            var objList = this.objects.Values.ToList();
            DependencySort(objList);

            foreach (var m in objList)
            {
                m.Initialize(db);
            }
        }

        private static void DependencySort(IList<IObjectService> objList)
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


    }
}
