using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

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
        private HashSet<string> loadedResources = new HashSet<string>();

        /// <summary>
        /// 初始化一个数据库环境
        /// </summary>
        /// <param name="dbName"></param>
        public DBProfile(string dbName)
        {
            Debug.Assert(!string.IsNullOrEmpty(dbName));

            this.Connection = DataProvider.CreateDataContext(dbName);

            this.EnsureInitialization();
        }

        public DBProfile(IDBConnection conn, IResourceContainer resources)
        {
            Debug.Assert(conn != null);
            Debug.Assert(resources != null);

            this.Connection = conn;
            this.Resources = resources;

            this.EnsureInitialization();
        }

        private void EnsureInitialization()
        {
            //如果数据库是一个新建的空数据库，那么我们就需要先初始化此数据库为一个 ObjectServer 账套数据库
            if (!this.Connection.IsInitialized())
            {
                this.Connection.Initialize();
            }
        }

        ~DBProfile()
        {
            this.Dispose(false);
        }

        public IDBConnection Connection { get; private set; }

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

            this.Connection.Dispose();
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
                var msg = string.Format("Cannot found resource: '{0}'", resName);
                Logger.Error(() => msg);

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
                LoadResource(res);
            }
        }

        private void LoadResource(IResource res)
        {
            Debug.Assert(res != null);

            if (!this.loadedResources.Contains(res.Name))
            {
                res.Load(this);
                this.loadedResources.Add(res.Name);
            }
        }

    }
}
