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
        private readonly IDictionary<string, int> resourceDependencyWeightMapping =
            new Dictionary<string, int>();
        private readonly IDictionary<string, IResource> resources =
            new Dictionary<string, IResource>();
        private readonly HashSet<string> initializedResources =
            new HashSet<string>();
        private bool disposed = false;

        /// <summary>
        /// 初始化一个数据库环境
        /// </summary>
        /// <param name="dbName"></param>
        public DBProfile(string dbName)
        {
            Debug.Assert(!string.IsNullOrEmpty(dbName));

        }

        public DBProfile(IResourceContainer resources)
        {
            Debug.Assert(resources != null);

            this.Resources = resources;

        }

        ~DBProfile()
        {
            this.Dispose(false);
        }

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

        public int GetResourceDependencyWeight(string resName)
        {
            if (string.IsNullOrEmpty(resName))
            {
                throw new ArgumentNullException("resName");
            }

            return this.resourceDependencyWeightMapping[resName];
        }

        #endregion

        public string DatabaseName { get; private set; }

        public void InitializeAllResources(IDBContext conn, bool update)
        {
            var allRes = new List<IResource>(this.resources.Values);
            ResourceDependencySort(allRes);

            for (int i = 0; i < allRes.Count; i++)
            {
                var res = allRes[i];
                this.InitializeResource(conn, res, i, update);
            }
        }

        private void InitializeResource(IDBContext conn, IResource res, int index, bool update)
        {
            Debug.Assert(res != null);

            if (!this.initializedResources.Contains(res.Name))
            {
                res.Initialize(conn, update);
                this.initializedResources.Add(res.Name);
                this.resourceDependencyWeightMapping.Add(res.Name, index);
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
