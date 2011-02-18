using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Transactions;
using Npgsql;

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

    }
}
