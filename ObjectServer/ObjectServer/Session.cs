using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

using ObjectServer.Backend;

namespace ObjectServer
{
    internal class Session : ISession
    {
        public Session(string dbName)
        {
            this.Database = new Database(dbName);
            this.Database.Open();
        }

        #region ISession 成员

        public Database Database
        {
            get;
            private set;
        }

        public ObjectPool Pool
        {
            get
            {
                return Pooler.Instance.GetPool(this.Database.DatabaseName);
            }
        }

        #endregion

        #region IDisposable 成员

        public void Dispose()
        {
            this.Database.Close();
        }

        #endregion
    }
}
