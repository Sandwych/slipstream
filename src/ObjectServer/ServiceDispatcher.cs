using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Reflection;

using Castle.DynamicProxy;

using ObjectServer.Backend;
using ObjectServer.Core;

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

                //用新 session 替换老 session
                Infrastructure.SessionStore.PutSession(session);
                Infrastructure.SessionStore.Remove(ctx.Session.Id);

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

        [CachedMethod(Timeout = 120)]
        public object Execute(string sessionId, string resource, string method, params object[] parameters)
        {
            var gsid = new Guid(sessionId);
            using (var scope = new ServiceScope(gsid))
            {
                dynamic res = scope.GetResource(resource);
                var svc = res.GetService(method);
                var internalArgs = new object[parameters.Length + 2];
                internalArgs[0] = res;
                internalArgs[1] = scope;
                parameters.CopyTo(internalArgs, 2);

                if (res.DatabaseRequired)
                {
                    return ExecuteTransactional(scope, svc, internalArgs);
                }
                else
                {
                    return svc.Invoke(internalArgs);
                }
            }
        }


        private static object ExecuteTransactional(
            IServiceScope scope, IService svc, params object[] internalArgs)
        {
            using (var tx = new TransactionScope())
            {
                var result = svc.Invoke(internalArgs);
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

            Infrastructure.DBProfiles.RemoveDB(dbName); //删除数据库上下文
            DataProvider.DeleteDatabase(dbName); //删除实际数据库
        }

        private static void VerifyRootPassword(string rootPasswordHash)
        {
            if (rootPasswordHash.ToUpperInvariant() !=
                Infrastructure.Configuration.RootPasswordHash.ToUpperInvariant())
            {
                throw new UnauthorizedAccessException("Invalid password of root user");
            }
        }

        #endregion


        #region Model methods

        [CachedMethod]
        public long CreateModel(string sessionId, string modelName, IDictionary<string, object> propertyBag)
        {
            return (long)Execute(sessionId, modelName, "Create", new object[] { propertyBag });
        }

        [CachedMethod]
        public long[] SearchModel(string sessionId, string modelName, object[] domain, object[] order, long offset, long limit)
        {
            return (long[])Execute(sessionId, modelName, "Search", new object[] { domain, order, offset, limit });
        }

        [CachedMethod]
        public Dictionary<string, object>[] ReadModel(string sessionId, string modelName, object[] ids, object[] fields)
        {
            return (Dictionary<string, object>[])Execute(
                sessionId, modelName, "Read", new object[] { ids, fields });
        }

        [CachedMethod]
        public void WriteModel(string sessionId, string modelName, object id, IDictionary<string, object> record)
        {
            Execute(sessionId, modelName, "Write", new object[] { id, record });
        }

        [CachedMethod]
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
