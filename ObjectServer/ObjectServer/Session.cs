using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Reflection;

using ObjectServer.Backend;

namespace ObjectServer
{
    internal class Session : ISession
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(
            MethodBase.GetCurrentMethod().DeclaringType);

        public Session(string dbName)
        {
            if (Log.IsInfoEnabled)
            {
                Log.InfoFormat("Session is opening for database: [{0}]", dbName);
            }

            this.Database = DataProvider.OpenDatabase(dbName);
        }

        public Session(IDatabaseContext db)
        {
            this.Database = db;
        }

        #region ISession 成员

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

            if (Log.IsInfoEnabled)
            {
                Log.InfoFormat("Session closed");
            }
        }

        #endregion
    }
}
