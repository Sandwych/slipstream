using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Reflection;

using ObjectServer.Backend;

namespace ObjectServer
{
    internal class CallingContext : ICallingContext
    {
        public CallingContext(string dbName)
        {
            Logger.Info(() => string.Format("CallingContext is opening for database: [{0}]", dbName));

            this.Database = DataProvider.OpenDatabase(dbName);
        }

        public CallingContext(IDatabaseContext db)
        {
            this.Database = db;
        }

        #region ICallingContext 成员

        public IDatabaseContext Database
        {
            get;
            private set;
        }

        public ObjectPool Pool
        {
            get
            {
                return ObjectServerStarter.Pooler.GetPool(this.Database.DatabaseName);
            }
        }

        #endregion

        #region IDisposable 成员

        public void Dispose()
        {
            this.Database.Close();

            Logger.Info(() => "CallingContext closed");
        }

        #endregion
    }
}
