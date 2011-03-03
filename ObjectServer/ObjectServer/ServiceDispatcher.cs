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
    public sealed class ServiceDispatcher : MarshalByRefObject, IExportedService
    {
        private ServiceDispatcher()
        {
        }


        public string LogOn(string dbName, string username, string password)
        {
            using (var callingContext = new ContextScope(dbName))
            {
                var userModel = callingContext.Database.Resources.Resolve(UserModel.ModelName);
                var method = userModel.GetServiceMethod("LogOn");
                return (string)ExecuteTransactional(
                    callingContext, userModel, method, callingContext, dbName, username, password);
            }
        }


        public void LogOff(string sessionId)
        {
            var sgid = new Guid(sessionId);
            using (var callingContext = new ContextScope(sgid))
            {
                callingContext.Database.DataContext.Open();

                var userModel = (UserModel)callingContext.Database.Resources.Resolve(UserModel.ModelName);
                userModel.LogOut(callingContext, sessionId);
            }
        }

        /*
        public string[] GetResourceNames(string sessionId)
        {
            var sgid = new Guid(sessionId);
            using (var callingContext = new ContextScope(sgid))
            {
                //callingContext.Database.DataContext.Open();
                callingContext.Database.ServiceObjects. 
                
                userModel.LogOut(callingContext, sessionId);
            }
        }*/


        public string GetVersion()
        {
            return StaticSettings.Version.ToString();
        }

        [CachedMethod(Timeout = 120)]
        public object Execute(string sessionId, string resource, string name, params object[] parameters)
        {
            var gsid = new Guid(sessionId);
            using (var callingContext = new ContextScope(gsid))
            {
                var obj = callingContext.Database.Resources.Resolve(resource);
                var method = obj.GetServiceMethod(name);
                var internalArgs = new object[parameters.Length + 1];
                internalArgs[0] = callingContext;
                parameters.CopyTo(internalArgs, 1);

                if (obj.DatabaseRequired)
                {
                    return ExecuteTransactional(callingContext, obj, method, internalArgs);
                }
                else
                {
                    return method.Invoke(obj, internalArgs);
                }
            }
        }


        private static object ExecuteTransactional(
            IContext ctx, IResource obj, MethodInfo method, params object[] internalArgs)
        {
            ctx.Database.DataContext.Open();

            using (var tx = new TransactionScope())
            {
                var result = method.Invoke(obj, internalArgs);
                tx.Complete();
                ctx.Database.DataContext.Close();
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

            using (var callingContext = new ContextScope(dbName))
            {
                callingContext.Database.DataContext.Initialize();
                ObjectServerStarter.Databases.LoadDatabase(dbName);
            }
        }

        public void DeleteDatabase(string rootPasswordHash, string dbName)
        {
            VerifyRootPassword(rootPasswordHash);

            ObjectServerStarter.Databases.RemoveDatabase(dbName); //删除数据库上下文
            DataProvider.DeleteDatabase(dbName); //删除实际数据库
        }

        private static void VerifyRootPassword(string rootPasswordHash)
        {
            if (rootPasswordHash.ToUpperInvariant() !=
                ObjectServerStarter.Configuration.RootPasswordHash.ToUpperInvariant())
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
        public object[] SearchModel(string sessionId, string modelName, object[] domain, long offset, long limit)
        {
            return (object[])Execute(sessionId, modelName, "Search", new object[] { domain, offset, limit });
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
            var generator = new ProxyGenerator();
            var target = new ServiceDispatcher();
            var proxy = generator.CreateInterfaceProxyWithTarget<IExportedService>(
                target, new MethodCachingInterceptor(), new ServiceMethodInterceptor());

            return proxy;
        }
    }
}
