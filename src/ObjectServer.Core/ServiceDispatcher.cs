using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Reflection;

using ObjectServer.Data;
using ObjectServer.Core;
using ObjectServer.Utility;

namespace ObjectServer
{
    internal sealed class ServiceDispatcher : IExportedService
    {
        private ServiceDispatcher()
        {
        }

        public string LogOn(string dbName, string username, string password)
        {
            using (var ctx = new ServiceContext(dbName, "system"))
            using (var tx = new TransactionScope())
            {
                ctx.DBContext.Open();

                dynamic userModel = ctx.GetResource(UserModel.ModelName);
                Session session = userModel.LogOn(ctx, dbName, username, password);

                if (session == null)
                {
                    throw new System.Security.SecurityException("Failed to logon");
                }

                //用新 session 替换老 session
                Environment.SessionStore.PutSession(session);
                Environment.SessionStore.Remove(ctx.Session.ID);

                tx.Complete();
                return session.ID.ToString();
            }
        }


        public void LogOff(string sessionId)
        {
            using (var ctx = new ServiceContext(sessionId))
            using (var tx = new TransactionScope())
            {
                ctx.DBContext.Open();

                dynamic userModel = ctx.GetResource(UserModel.ModelName);
                userModel.LogOut(ctx, sessionId);
            }
        }

        public string GetVersion()
        {
            return StaticSettings.Version.ToString();
        }

        public object Execute(string sessionId, string resource, string method, params object[] args)
        {

            using (var scope = new ServiceContext(sessionId))
            {
                dynamic res = scope.GetResource(resource);
                var svc = res.GetService(method);

                if (res.DatabaseRequired)
                {
                    return ExecuteTransactional(scope, res, svc, args);
                }
                else
                {
                    return svc.Invoke(res, scope, args);
                }
            }
        }

        private static object ExecuteTransactional(ServiceContext scope, dynamic res, dynamic svc, object[] args)
        {
            var tx = scope.DBContext.BeginTransaction();
            try
            {
                var result = svc.Invoke(res, scope, args);
                tx.Commit();
                return result;
            }
            catch
            {
                tx.Rollback();
                throw;
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
            Environment.DBProfiles.LoadDB(dbName);
        }

        public void DeleteDatabase(string rootPasswordHash, string dbName)
        {
            VerifyRootPassword(rootPasswordHash);

            Environment.DBProfiles.RemoveDB(dbName); //删除数据库上下文
            DataProvider.DeleteDatabase(dbName); //删除实际数据库
        }

        private static void VerifyRootPassword(string rootPasswordHash)
        {
            var cfgRootPasswordHash = Environment.Configuration.RootPassword.ToSha();
            if (rootPasswordHash != cfgRootPasswordHash)
            {
                throw new ObjectServer.Exceptions.SecurityException("Invalid password of root user");
            }
        }

        #endregion


        #region Model methods

        public long CreateModel(string sessionId, string modelName, IDictionary<string, object> propertyBag)
        {
            return (long)Execute(sessionId, modelName, "Create", new object[] { propertyBag });
        }

        public long CountModel(string sessionId, string modelName, object[] constraints)
        {
            return (long)Execute(sessionId, modelName, "Count", new object[] { constraints });
        }

        public long[] SearchModel(string sessionId, string modelName, object[] constraints, object[] order, long offset, long limit)
        {
            return (long[])Execute(sessionId, modelName, "Search", new object[] { constraints, order, offset, limit });
        }

        public Dictionary<string, object>[] ReadModel(string sessionId, string modelName, object[] ids, object[] fields)
        {
            return (Dictionary<string, object>[])Execute(
                sessionId, modelName, "Read", new object[] { ids, fields });
        }

        public void WriteModel(string sessionId, string modelName, object id, IDictionary<string, object> record)
        {
            Execute(sessionId, modelName, "Write", new object[] { id, record });
        }

        public void DeleteModel(string sessionId, string modelName, object[] ids)
        {
            Execute(sessionId, modelName, "Delete", new object[] { ids });
        }

        #endregion


        //Factory method

        public static IExportedService CreateDispatcher()
        {
            return new ServiceDispatcher();
        }
    }
}
