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
        private HashSet<string> loadedResources = new HashSet<string>();

        /// <summary>
        /// 初始化一个数据库环境
        /// </summary>
        /// <param name="dbName"></param>
        public Database(string dbName)
        {
            this.DataContext = DataProvider.CreateDataContext(dbName);
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

        private static void ResourceDependencySort(IList<IResource> resList)
        {
            Debug.Assert(resList != null);

            var objDepends = new Dictionary<string, string[]>();
            foreach (var res in resList)
            {
                objDepends.Add(res.Name, res.GetReferencedObjects());
            }

            resList.DependencySort(m => m.Name, m => objDepends[m.Name]);
        }

        public string DatabaseName { get; private set; }

        public void InitializeAllResources()
        {
            var allRes = new List<IResource>(this.resources.Values);
            ResourceDependencySort(allRes);

            foreach (var res in allRes)
            {
                if (!this.loadedResources.Contains(res.Name))
                {
                    res.Load(this);
                    this.loadedResources.Add(res.Name);
                }
            }
        }

    }
}
