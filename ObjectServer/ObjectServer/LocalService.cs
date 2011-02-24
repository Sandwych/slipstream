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
    public sealed class LocalService : MarshalByRefObject, IService
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

        public void LogOff(string dbName, string sessionId)
        {
            using (var callingContext = new CallingContext(dbName))
            {
                callingContext.Database.Open();

                var userModel = (UserModel)callingContext.Pool[UserModel.ModelName];
                userModel.LogOut(callingContext, sessionId);
            }
        }

        public object Execute(string sessionId, string objectName, string name, params object[] args)
        {
            var gsid = new Guid(sessionId);
            using (var callingContext = new CallingContext(gsid))
            {
                var obj = callingContext.Pool[objectName];
                var method = obj.GetServiceMethod(name);
                var internalArgs = new object[args.Length + 1];
                internalArgs[0] = callingContext;
                args.CopyTo(internalArgs, 1);

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
            callingContext.Database.Open();

            var tx = callingContext.Database.Connection.BeginTransaction();
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
            using (var db = DataProvider.OpenDatabase())
            {
                return db.List();
            }
        }

        public void CreateDatabase(string rootPasswordHash, string dbName, string adminPassword)
        {
            VerifyRootPassword(rootPasswordHash);

            using (var db = DataProvider.OpenDatabase())
            {
                db.Create(dbName);
            }

            using (var callingContext = new CallingContext(dbName))
            {
                callingContext.Database.Initialize();
                ObjectServerStarter.Pooler.RegisterDatabase(dbName);
            }
        }

        public void DeleteDatabase(string rootPasswordHash, string dbName)
        {
            VerifyRootPassword(rootPasswordHash);

            using (var db = DataProvider.OpenDatabase())
            {
                db.Delete(dbName);
            }
        }

        private static void VerifyRootPassword(string rootPasswordHash)
        {
            if (rootPasswordHash.ToUpperInvariant() !=
                ObjectServerStarter.Configuration.RootPasswordHash.ToUpperInvariant())
            {
                throw new UnauthorizedAccessException("Invalid root password");
            }
        }

        #endregion


        #region Model methods


        public long CreateModel(string dbName, string objectName, IDictionary<string, object> propertyBag)
        {
            return (long)Execute(dbName, objectName, "Create", new object[] { propertyBag });
        }

        public object[] SearchModel(string dbName, string objectName, object[][] domain, long offset, long limit)
        {
            return (object[])Execute(dbName, objectName, "Search", new object[] { domain, offset, limit });
        }

        public Dictionary<string, object>[] ReadModel(string dbName, string objectName, object[] ids, object[] fields)
        {
            return (Dictionary<string, object>[])Execute(
                dbName, objectName, "Read", new object[] { ids, fields });
        }

        public void WriteModel(string dbName, string objectName, object id, IDictionary<string, object> record)
        {
            Execute(dbName, objectName, "Write", new object[] { id, record });
        }

        public void DeleteModel(string dbName, string objectName, object[] ids)
        {
            Execute(dbName, objectName, "Delete", new object[] { ids });
        }

        #endregion
    }
}
