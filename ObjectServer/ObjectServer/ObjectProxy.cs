using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Transactions;
using Npgsql;

namespace ObjectServer
{
    public class ObjectProxy : MarshalByRefObject
    {

        public object Execute(string objectName, string name, object[] args)
        {
            string connectionString =
                "Server=localhost;" +
                "Database=objectserver;" +
                "Encoding=UNICODE;" +
                "User ID=postgres;" +
                "Password=postgres;";
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                var session = new Session(conn);

                var model = (ModelBase)ObjectPool.Instance.LookupObject(objectName);
                var method = model.GetServiceMethod(name);
                var internalArgs = new object[args.Length + 1];
                internalArgs[0] = session;
                args.CopyTo(internalArgs, 1);

                NpgsqlTransaction tx = conn.BeginTransaction();
                try
                {
                    var result = method.Invoke(model, internalArgs);
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
}
