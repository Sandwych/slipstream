using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Reflection;

using ObjectServer.Backend;
using ObjectServer.Core;

namespace ObjectServer
{
    public sealed class ServiceDispatcher : IService
    {
        public string LogOn(string dbName, string username, string password)
        {
            using (var callingContext = new CallingContext(dbName))
            {
                var userModel = callingContext.Pool[UserModel.ModelName];
                var method = userModel.GetServiceMethod("LogOn");
                return (string)ExecuteTransactional(
                    callingContext, userModel, method, callingContext, dbName, username, password);
            }
        }

        public void LogOff(string sessionId)
        {
            var sgid = new Guid(sessionId);
            using (var callingContext = new CallingContext(sgid))
            {
                callingContext.DatabaseContext.Open();

                var userModel = (UserModel)callingContext.Pool[UserModel.ModelName];
                userModel.LogOut(callingContext, sessionId);
            }
        }

        public string GetVersion()
        {
            return StaticSettings.Version.ToString();
        }

        public object Execute(string sessionId, string objectName, string name, params object[] parameters)
        {
            var gsid = new Guid(sessionId);
            using (var callingContext = new CallingContext(gsid))
            {
                var obj = callingContext.Pool[objectName];
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
            CallingContext callingContext, IServiceObject obj, MethodInfo method, params object[] internalArgs)
        {
            callingContext.DatabaseContext.Open();

            var tx = callingContext.DatabaseContext.Connection.BeginTransaction();
            try
            {
                var result = method.Invoke(obj, internalArgs);
                tx.Commit();
                return result;
            }
            catch (Exception ex)
            {
                tx.Rollback();
                throw ex;
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

            using (var callingContext = new CallingContext(dbName))
            {
                callingContext.DatabaseContext.Initialize();
                ObjectServerStarter.DatabasePool.RegisterDatabase(dbName);
            }
        }

        public void DeleteDatabase(string rootPasswordHash, string dbName)
        {
            VerifyRootPassword(rootPasswordHash);
            DataProvider.DeleteDatabase(dbName);
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


        public long CreateModel(string sessionId, string objectName, IDictionary<string, object> propertyBag)
        {
            return (long)Execute(sessionId, objectName, "Create", new object[] { propertyBag });
        }

        public object[] SearchModel(string sessionId, string objectName, object[] domain, long offset, long limit)
        {
            return (object[])Execute(sessionId, objectName, "Search", new object[] { domain, offset, limit });
        }

        public Dictionary<string, object>[] ReadModel(string sessionId, string objectName, object[] ids, object[] fields)
        {
            return (Dictionary<string, object>[])Execute(
                sessionId, objectName, "Read", new object[] { ids, fields });
        }

        public void WriteModel(string sessionId, string objectName, object id, IDictionary<string, object> record)
        {
            Execute(sessionId, objectName, "Write", new object[] { id, record });
        }

        public void DeleteModel(string sessionId, string objectName, object[] ids)
        {
            Execute(sessionId, objectName, "Delete", new object[] { ids });
        }

        #endregion
    }
}
