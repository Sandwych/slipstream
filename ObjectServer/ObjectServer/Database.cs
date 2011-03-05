using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using ObjectServer.Utility;
using ObjectServer.Backend;

namespace ObjectServer
{
    public class Database : IDatabase
    {
        private IDictionary<string, IResource> resources = new Dictionary<string, IResource>();

        /// <summary>
        /// 初始化一个数据库环境
        /// </summary>
        /// <param name="dbName"></param>
        public Database(string dbName)
        {
            this.DataContext = DataProvider.CreateDataContext(dbName);

            lock (this)
            {
                this.LoadAllCoreObjects(this);
                this.InitializeRegisteredObjects(this);
            }

        }

        public Database(IDataContext dataCtx, IResourceContainer resources)
        {
            this.DataContext = dataCtx;
            this.Resources = resources;
        }

        ~Database()
        {
            this.Dispose(false);
        }

        public IDataContext DataContext { get; private set; }

        public IResourceContainer Resources { get; private set; }

        #region IDisposable 成员

        public void Dispose()
        {
            this.Dispose(true);
        }

        public void Dispose(bool disposing)
        {
            if (disposing)
            {
                //这里处理托管对象
            }

            this.DataContext.Dispose();
        }

        #endregion

        #region IResourceContainer 成员

        public void RegisterResource(IResource res)
        {
            lock (this)
            {
                this.resources.Add(res.Name, res);
            }
        }

        public dynamic GetResource(string resName)
        {
            if (!this.resources.ContainsKey(resName))
            {
                var msg = string.Format("Cannot found resource: '{0}'", resName);
                Logger.Error(() => msg);

                throw new ResourceNotFoundException(msg, resName);
            }

            return this.resources[resName];
        }

        public dynamic this[string resName]
        {
            get { return this.GetResource(resName); }
        }

        #endregion

        private void LoadAllCoreObjects(IDatabase db)
        {
            Debug.Assert(db != null);

            this.resources.Clear();
            Module.RegisterAllCoreObjects(db);

        }

        private void InitializeRegisteredObjects(IDatabase db)
        {
            Debug.Assert(db != null);

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
