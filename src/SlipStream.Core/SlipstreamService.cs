using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Reflection;
using System.Diagnostics;

using Sandwych.Utility;
using Sandwych;
using SlipStream.Data;
using SlipStream.Core;

namespace SlipStream
{
    using IRecord = IDictionary<string, object>;
    using Record = Dictionary<string, object>;

    internal sealed class SlipstreamService : ISlipstreamService
    {
        private readonly IDataProvider _dataProvider;
        private readonly IDbDomainManager _dbDomainManager;

        public SlipstreamService(IDataProvider dataProvider, IDbDomainManager dbDomainManager)
        {
            this._dataProvider = dataProvider;
            this._dbDomainManager = dbDomainManager;
        }

        public string LogOn(string dbName, string username, string password)
        {
            if (string.IsNullOrEmpty(dbName))
            {
                throw new ArgumentNullException(nameof(dbName));
            }

            if (string.IsNullOrEmpty(username))
            {
                throw new ArgumentNullException(nameof(username));
            }

            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentNullException(nameof(password));
            }

            var dbDomain = _dbDomainManager.GetDbDomain(dbName);
            using (var ctx = dbDomain.OpenSystemSession())
            {
                dynamic userEntity = dbDomain.GetResource(UserEntity.EntityName);
                UserSession session = userEntity.LogOn(dbName, username, password);
                Debug.Assert(session != null);
                return session.Token.ToString();
            }
        }


        public void LogOff(string db, string sessionToken)
        {
            if (string.IsNullOrEmpty(db))
            {
                throw new ArgumentNullException(nameof(db));
            }

            if (string.IsNullOrEmpty(sessionToken))
            {
                throw new ArgumentNullException(nameof(sessionToken));
            }

            var dbDomain = _dbDomainManager.GetDbDomain(db);
            using (var ctx = dbDomain.OpenSession(sessionToken))
            {
                dynamic userEntity = dbDomain.GetResource(UserEntity.EntityName);
                userEntity.LogOff(sessionToken);
            }
        }

        public string GetVersion()
        {
            return StaticSettings.Version.ToString();
        }

        public object Execute(string db, string sessionToken, string resource, string method, params object[] args)
        {
            if (string.IsNullOrEmpty(db))
            {
                throw new ArgumentNullException(nameof(db));
            }

            if (string.IsNullOrEmpty(sessionToken))
            {
                throw new ArgumentNullException(nameof(sessionToken));
            }

            if (string.IsNullOrEmpty(resource))
            {
                throw new ArgumentNullException(nameof(resource));
            }

            if (string.IsNullOrEmpty(method))
            {
                throw new ArgumentNullException(nameof(method));
            }

            //加入事务
            //REVIEW
            var dbDomain = _dbDomainManager.GetDbDomain(db);
            using (var ctx = dbDomain.OpenSession(sessionToken))
            {
                dynamic res = dbDomain.GetResource(resource);
                var svc = res.GetService(method);
                var result = svc.Invoke(res, args);
                return result;
            }
        }

        #region Database handling methods

        public string[] ListDatabases()
        {
            return this._dataProvider.ListDatabases();
        }

        public void CreateDatabase(string rootPasswordHash, string dbName, string adminPassword)
        {
            VerifyRootPassword(rootPasswordHash);

            this._dataProvider.CreateDatabase(dbName);
            SlipstreamEnvironment.DbDomains.Register(dbName, true);
        }

        public void DeleteDatabase(string rootPasswordHash, string dbName)
        {
            VerifyRootPassword(rootPasswordHash);

            this._dataProvider.DeleteDatabase(dbName); //删除实际数据库
            SlipstreamEnvironment.DbDomains.Remove(dbName); //删除数据库上下文
        }

        private static void VerifyRootPassword(string rootPasswordHash)
        {
            var cfgRootPasswordHash = SlipstreamEnvironment.Settings.ServerPassword.ToSha();
            if (rootPasswordHash.ToLowerInvariant() != cfgRootPasswordHash.ToLowerInvariant())
            {
                throw new SlipStream.Exceptions.SecurityException("Invalid password of root user");
            }
        }

        #endregion


        public long CreateEntity(string db, string sessionToken, string entityName, IRecord record)
        {
            return (long)Execute(db, sessionToken, entityName, "Create", new object[] { record });
        }

        public long CountEntity(string db, string sessionToken, string entityName, object[] constraints)
        {
            return (long)Execute(db, sessionToken, entityName, "Count", new object[] { constraints });
        }

        public long[] SearchEntity(
            string db, string sessionToken, string entityName, object[] constraints, object[] order, long offset, long limit)
        {
            return (long[])Execute(db, sessionToken, entityName, "Search", new object[] { constraints, order, offset, limit });
        }

        public Record[] ReadEntity(string db, string sessionToken, string entityName, object[] ids, object[] fields)
        {
            return (Record[])Execute(db, sessionToken, entityName, "Read", new object[] { ids, fields });
        }

        public void WriteEntity(
            string db, string sessionToken, string entityName, object id, IRecord record)
        {
            Execute(db, sessionToken, entityName, "Write", new object[] { id, record });
        }

        public void DeleteEntity(string db, string sessionToken, string entityName, object[] ids)
        {
            Execute(db, sessionToken, entityName, "Delete", new object[] { ids });
        }

    }
}
