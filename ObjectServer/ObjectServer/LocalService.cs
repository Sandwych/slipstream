using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Transactions;

using ObjectServer.Backend;

namespace ObjectServer
{
    public sealed class LocalService : IService
    {
        public object Execute(string dbName, string objectName, string name, params object[] args)
        {
            using (var session = new Session(dbName))
            {
                var obj = session.Pool.LookupObject(objectName);
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
            catch (Exception ex)
            {
                tx.Rollback();
                throw ex;
            }
        }


        #region Database handling methods

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

    }
}
