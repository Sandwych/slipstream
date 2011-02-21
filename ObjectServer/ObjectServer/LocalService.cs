using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Transactions;

using ObjectServer.Backend;

namespace ObjectServer
{
    public sealed class LocalService : MarshalByRefObject, IService
    {
        public Guid Login(string dbName, string username, string password)
        {
            throw new NotImplementedException();
        }

        public void Logout(string dbName, Guid session)
        {
            throw new NotImplementedException();
        }

        public object Execute(string dbName, string objectName, string name, params object[] args)
        {
            using (var session = new Session(dbName))
            {
                var obj = session.Pool[objectName];
                var method = obj.GetServiceMethod(name);
                var internalArgs = new object[args.Length + 1];
                internalArgs[0] = session;
                args.CopyTo(internalArgs, 1);

                if (obj.DatabaseRequired)
                {
                    return ExecuteTransactional(session, obj, method, internalArgs);
                }
                else
                {
                    return method.Invoke(obj, internalArgs);
                }
            }
        }

        private static object ExecuteTransactional(
            Session session, IServiceObject obj, System.Reflection.MethodInfo method, object[] internalArgs)
        {
            session.Database.Open();

            var tx = session.Database.Connection.BeginTransaction();
            try
            {
                var result = method.Invoke(obj, internalArgs);
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

            using (var session = new Session(dbName))
            {
                session.Database.Initialize();
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

        public long[] SearchModel(string dbName, string objectName, object[][] domain, long offset, long limit)
        {
            return (long[])Execute(dbName, objectName, "Search", new object[] { domain, offset, limit });
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
