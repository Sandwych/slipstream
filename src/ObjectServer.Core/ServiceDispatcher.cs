using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Reflection;
using System.Diagnostics;

using ObjectServer.Data;
using ObjectServer.Core;
using ObjectServer.Utility;

namespace ObjectServer
{
    using IRecord = IDictionary<string, object>;
    using Record = Dictionary<string, object>;

    internal sealed class ServiceDispatcher : IExportedService
    {
        public string LogOn(string dbName, string username, string password)
        {
            if (string.IsNullOrEmpty(dbName))
            {
                throw new ArgumentNullException("dbName");
            }

            if (string.IsNullOrEmpty(username))
            {
                throw new ArgumentNullException("username");
            }

            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentNullException("password");
            }

            using (var ctx = new ServiceContext(dbName))
            {
                dynamic userModel = ctx.GetResource(UserModel.ModelName);
                Session session = userModel.LogOn(ctx, dbName, username, password);
                Debug.Assert(session != null);
                return session.Id.ToString();
            }
        }


        public void LogOff(string db, string sessionId)
        {
            if (string.IsNullOrEmpty(db))
            {
                throw new ArgumentNullException("db");
            }

            if (string.IsNullOrEmpty(sessionId))
            {
                throw new ArgumentNullException("sessionId");
            }

            using (var ctx = new ServiceContext(db, sessionId))
            {

                dynamic userModel = ctx.GetResource(UserModel.ModelName);
                userModel.LogOff(ctx, sessionId);
            }
        }

        public string GetVersion()
        {
            return StaticSettings.Version.ToString();
        }

        public object Execute(string db, string sessionId, string resource, string method, params object[] args)
        {
            if (string.IsNullOrEmpty(db))
            {
                throw new ArgumentNullException("db");
            }

            if (string.IsNullOrEmpty(sessionId))
            {
                throw new ArgumentNullException("sessionId");
            }

            if (string.IsNullOrEmpty(resource))
            {
                throw new ArgumentNullException("resource");
            }

            //加入事务
            using (var txScope = new System.Transactions.TransactionScope())
            using (var svcCtx = new ServiceContext(db, sessionId))
            {
                dynamic res = svcCtx.GetResource(resource);
                var svc = res.GetService(method);
                var result = svc.Invoke(res, svcCtx, args);
                txScope.Complete();
                return result;
            }
        }

        #region Database handling methods

        public string[] ListDatabases()
        {
            return DataProvider.ListDatabases();
        }

        public void CreateDatabase(string rootPasswordHash, string dbName, string adminPassword)
        {
            VerifyRootPassword(rootPasswordHash);

            DataProvider.CreateDatabase(dbName);
            Environment.DBProfiles.Register(dbName, true);
        }

        public void DeleteDatabase(string rootPasswordHash, string dbName)
        {
            VerifyRootPassword(rootPasswordHash);

            DataProvider.DeleteDatabase(dbName); //删除实际数据库
            Environment.DBProfiles.Remove(dbName); //删除数据库上下文
        }

        private static void VerifyRootPassword(string rootPasswordHash)
        {
            var cfgRootPasswordHash = Environment.Configuration.ServerPassword.ToSha();
            if (rootPasswordHash != cfgRootPasswordHash)
            {
                throw new ObjectServer.Exceptions.SecurityException("Invalid password of root user");
            }
        }

        #endregion


        #region Model methods

        public long CreateModel(string db, string sessionId, string modelName, IRecord record)
        {
            return (long)Execute(db, sessionId, modelName, "Create", new object[] { record });
        }

        public long CountModel(string db, string sessionId, string modelName, object[] constraints)
        {
            return (long)Execute(db, sessionId, modelName, "Count", new object[] { constraints });
        }

        public long[] SearchModel(
            string db, string sessionId, string modelName, object[] constraints, object[] order, long offset, long limit)
        {
            return (long[])Execute(db, sessionId, modelName, "Search", new object[] { constraints, order, offset, limit });
        }

        public Record[] ReadModel(string db, string sessionId, string modelName, object[] ids, object[] fields)
        {
            return (Record[])Execute(db, sessionId, modelName, "Read", new object[] { ids, fields });
        }

        public void WriteModel(
            string db, string sessionId, string modelName, object id, IRecord record)
        {
            Execute(db, sessionId, modelName, "Write", new object[] { id, record });
        }

        public void DeleteModel(string db, string sessionId, string modelName, object[] ids)
        {
            Execute(db, sessionId, modelName, "Delete", new object[] { ids });
        }

        #endregion


        //Factory method

        public static IExportedService CreateDispatcher()
        {
            return new ServiceDispatcher();
        }
    }
}
