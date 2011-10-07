using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;

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
    internal class DbProfile : IDbProfile
    {
        private readonly IDictionary<string, int> resourceDependencyWeightMapping =
            new Dictionary<string, int>();
        private readonly IDictionary<string, IResource> resources =
            new Dictionary<string, IResource>();
        private readonly HashSet<string> initializedResources =
            new HashSet<string>();
        private bool disposed = false;
        private readonly ReaderWriterLockSlim resourcesLock = new ReaderWriterLockSlim();
        private readonly string dbName;

        /// <summary>
        /// 初始化一个数据库环境
        /// </summary>
        public DbProfile(string db)
        {
            if (string.IsNullOrEmpty(db))
            {
                throw new ArgumentNullException("db");
            }

            this.dbName = db;
        }

        ~DbProfile()
        {
            this.Dispose(false);
        }

        public void Initialize(bool isUpdate)
        {
            using (var ctx = new TransactionContext(this.DatabaseName, this))
            {
                Environment.Modules.UpdateModuleList(ctx.DBContext);

                //加载其它模块
                Environment.Modules.LoadModules(ctx, isUpdate);
            }
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

            this.resourcesLock.EnterWriteLock();
            try
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
            finally
            {
                this.resourcesLock.ExitWriteLock();
            }
        }

        public IResource GetResource(string resName)
        {
            Debug.Assert(!string.IsNullOrEmpty(resName));

            this.resourcesLock.EnterReadLock();
            try
            {
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
            finally
            {
                this.resourcesLock.ExitReadLock();
            }
        }

        public bool ContainsResource(string resName)
        {
            if (string.IsNullOrEmpty(resName))
            {
                throw new ArgumentNullException("resName");
            }

            this.resourcesLock.EnterReadLock();
            try
            {
                return this.resources.ContainsKey(resName);
            }
            finally
            {
                this.resourcesLock.ExitReadLock();
            }
        }

        public int GetResourceDependencyWeight(string resName)
        {
            if (string.IsNullOrEmpty(resName))
            {
                throw new ArgumentNullException("resName");
            }

            this.resourcesLock.EnterReadLock();
            try
            {
                return this.resourceDependencyWeightMapping[resName];
            }
            finally
            {
                this.resourcesLock.ExitReadLock();
            }
        }

        public IResource[] GetAllResources()
        {
            return this.resources.Values.ToArray();
        }

        #endregion

        public string DatabaseName
        {
            get
            {
                Debug.Assert(!string.IsNullOrEmpty(this.dbName));
                return this.dbName;
            }
        }

        public void InitializeAllResources(ITransactionContext tc, bool update)
        {
            IList<IResource> allRes = null;
            this.resourcesLock.EnterReadLock();
            try
            {
                allRes = new List<IResource>(this.resources.Values);
            }
            finally
            {
                this.resourcesLock.ExitReadLock();
            }

            ResourceDependencySort(allRes);

            for (int i = 0; i < allRes.Count; i++)
            {
                var res = allRes[i];
                this.InitializeResource(tc, res, i, update);
            }
        }


        private void InitializeResource(ITransactionContext tc, IResource res, int index, bool update)
        {
            Debug.Assert(res != null);

            if (!this.initializedResources.Contains(res.Name))
            {
                res.Initialize(tc, update);
                this.resourcesLock.EnterWriteLock();
                try
                {
                    this.initializedResources.Add(res.Name);
                    this.resourceDependencyWeightMapping.Add(res.Name, index);
                }
                finally
                {
                    this.resourcesLock.ExitWriteLock();
                }
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
