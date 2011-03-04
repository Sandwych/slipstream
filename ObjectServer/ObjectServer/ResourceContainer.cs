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

    public sealed class ResourceContainer : IResourceContainer
    {
        private IDictionary<string, IResource> resources =
            new Dictionary<string, IResource>();

        private IDatabase database;
        private bool initialized;

        public ResourceContainer(IDatabase db)
        {
            this.database = db;
            this.initialized = false;
        }

        #region IResourceContainer 成员

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

        public void RegisterResource(IResource res)
        {
            Debug.Assert(!this.initialized);

            lock (this)
            {
                this.resources.Add(res.Name, res);
            }
        }


        public dynamic Resolve(string resName)
        {
            Debug.Assert(this.initialized);

            if (!this.resources.ContainsKey(resName))
            {
                var msg = string.Format("Cannot found service object: '{0}'", resName);
                Logger.Error(() => msg);

                throw new ServiceObjectNotFoundException(msg, resName);
            }

            return this.resources[resName];
        }

        public dynamic this[string resName]
        {
            get { return this.Resolve(resName); }
        }

        #endregion

        private void LoadAllObjects(IDatabase db)
        {
            Debug.Assert(db != null);
            Debug.Assert(!this.initialized);

            this.resources.Clear();
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
            var objList = this.resources.Values.ToList();
            DependencySort(objList);

            foreach (var m in objList)
            {
                m.Initialize(db);
            }
        }

        private static void DependencySort(IList<IResource> objList)
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
