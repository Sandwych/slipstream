using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using ObjectServer.Exceptions;
using ObjectServer.Utility;
using ObjectServer.Data;

namespace ObjectServer
{
    /// <summary>
    /// 数据库
    /// 用于描述一个帐套数据库的上下文环境
    /// 一个数据库包含了该数据库中的所有对象
    /// </summary>
    internal class DBProfile : IDBProfile
    {
        private IDictionary<string, IResource> resources = new Dictionary<string, IResource>();
        private HashSet<string> initializedResources = new HashSet<string>();
        private bool disposed = false;

        /// <summary>
        /// 初始化一个数据库环境
        /// </summary>
        /// <param name="dbName"></param>
        public DBProfile(string dbName)
        {
            Debug.Assert(!string.IsNullOrEmpty(dbName));

            this.DBContext = DataProvider.CreateDataContext(dbName);

            this.EnsureInitialization();
        }

        public DBProfile(IDBContext dbctx, IResourceContainer resources)
        {
            Debug.Assert(dbctx != null);
            Debug.Assert(resources != null);

            this.DBContext = dbctx;
            this.Resources = resources;

            this.EnsureInitialization();
        }

        private void EnsureInitialization()
        {
            if (!this.DBContext.IsInitialized())
            {
                throw new DatabaseNotFoundException(
                    "Uninitialized database", this.DBContext.DatabaseName);
            }
        }

        ~DBProfile()
        {
            this.Dispose(false);
        }

        public IDBContext DBContext { get; private set; }

        public IResourceContainer Resources { get; private set; }

        #region IDisposable 成员

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    //这里处理托管对象
                }

                this.DBContext.Dispose();
                this.disposed = true;
            }
        }

        #endregion

        #region IResourceContainer 成员

        public void RegisterResource(IResource res)
        {
            Debug.Assert(res != null);

            if (res == null)
            {
                throw new ArgumentNullException("res");
            }

            lock (this)
            {
                IResource extendedRes;
                //先看是否存在               
                if (this.resources.TryGetValue(res.Name, out extendedRes))
                {
                    //处理单表继承（扩展）
                    extendedRes.MergeFrom(res);
                }
                else
                {
                    this.resources.Add(res.Name, res);
                }
            }
        }

        public IResource GetResource(string resName)
        {
            Debug.Assert(!string.IsNullOrEmpty(resName));

            IResource res;
            if (this.resources.TryGetValue(resName, out res))
            {
                return res;
            }
            else
            {
                var msg = string.Format("Cannot found resource: [{0}]", resName);
                LoggerProvider.EnvironmentLogger.Error(() => msg);

                throw new ResourceNotFoundException(msg, resName);
            }
        }

        public bool ContainsResource(string resName)
        {
            if (string.IsNullOrEmpty(resName))
            {
                throw new ArgumentNullException("resName");
            }

            return this.resources.ContainsKey(resName);
        }

        #endregion

        public string DatabaseName { get; private set; }

        public void InitializeAllResources(bool update)
        {
            var allRes = new List<IResource>(this.resources.Values);
            ResourceDependencySort(allRes);

            foreach (var res in allRes)
            {
                this.InitializeResource(res, update);
            }
        }

        private void InitializeResource(IResource res, bool update)
        {
            Debug.Assert(res != null);

            if (!this.initializedResources.Contains(res.Name))
            {
                res.Initialize(this, update);
                this.initializedResources.Add(res.Name);
            }
        }


        private static void ResourceDependencySort(IList<IResource> resList)
        {
            Debug.Assert(resList != null);

            var objDepends = new Dictionary<string, string[]>(resList.Count);
            foreach (var res in resList)
            {
                objDepends.Add(res.Name, res.GetReferencedObjects());
            }

            resList.DependencySort(m => m.Name, m => objDepends[m.Name]);
        }
    }
}
