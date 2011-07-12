using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Reflection;

using Castle.DynamicProxy;

using ObjectServer.Backend;
using ObjectServer.Core;
using ObjectServer.Utility;

namespace ObjectServer
{
    internal sealed class ServiceDispatcher : MarshalByRefObject, IExportedService
    {
        private ServiceDispatcher()
        {
        }

        public string LogOn(string dbName, string username, string password)
        {
            using (var ctx = new ServiceScope(dbName))
            using (var tx = new TransactionScope())
            {
                ctx.Connection.Open();

                dynamic userModel = ctx.GetResource(UserModel.ModelName);
                Session session = userModel.LogOn(ctx, dbName, username, password);

                if (session == null)
                {
                    throw new System.Security.SecurityException("Failed to logon");
                }

                //用新 session 替换老 session
                Platform.SessionStore.PutSession(session);
                Platform.SessionStore.Remove(ctx.Session.Id);

                tx.Complete();
                return session.Id.ToString();
            }
        }


        public void LogOff(string sessionId)
        {
            var sgid = new Guid(sessionId);
            using (var ctx = new ServiceScope(sgid))
            using (var tx = new TransactionScope())
            {
                ctx.Connection.Open();

                dynamic userModel = ctx.GetResource(UserModel.ModelName);
                userModel.LogOut(ctx, sessionId);
            }
        }

        /*
        public string[] GetResourceNames(string sessionId)
        {
            var sgid = new Guid(sessionId);
            using (var ctx = new ContextScope(sgid))
            {
                //ctx.Database.DataContext.Open();
                ctx.Database.ServiceObjects. 
                
                userModel.LogOut(ctx, sessionId);
            }
        }*/


        public string GetVersion()
        {
            return StaticSettings.Version.ToString();
        }

        public object Execute(string sessionId, string resource, string method, params object[] parameters)
        {
            var gsid = new Guid(sessionId);
            using (var scope = new ServiceScope(gsid))
            {
                dynamic res = scope.GetResource(resource);
                var svc = res.GetService(method);

                if (res.DatabaseRequired)
                {
                    return ExecuteTransactional(res, scope, svc, parameters);
                }
                else
                {
                    return svc.Invoke(res, scope, parameters);
                }
            }
        }


        private static object ExecuteTransactional(
            IResource res, IServiceScope scope, IService svc, params object[] args)
        {
            using (var tx = new TransactionScope())
            {
                var result = svc.Invoke(res, scope, args);
                tx.Complete();
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

        }

        public void DeleteDatabase(string rootPasswordHash, string dbName)
        {
            VerifyRootPassword(rootPasswordHash);

            Platform.DBProfiles.RemoveDB(dbName); //删除数据库上下文
            DataProvider.DeleteDatabase(dbName); //删除实际数据库
        }

        private static void VerifyRootPassword(string rootPasswordHash)
        {
            var cfgRootPasswordHash = Platform.Configuration.RootPassword.ToSha();
            if (rootPasswordHash != cfgRootPasswordHash)
            {
                throw new UnauthorizedAccessException("Invalid password of root user");
            }
        }

        #endregion


        #region Model methods

        public long CreateModel(string sessionId, string modelName, IDictionary<string, object> propertyBag)
        {
            return (long)Execute(sessionId, modelName, "Create", new object[] { propertyBag });
        }

        public long[] SearchModel(string sessionId, string modelName, object[] domain, object[] order, long offset, long limit)
        {
            return (long[])Execute(sessionId, modelName, "Search", new object[] { domain, order, offset, limit });
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
