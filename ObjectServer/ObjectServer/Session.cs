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
        private Database db;

        public Session(string dbName)
        {
            this.Database = dbName;
            this.db = new Database(dbName);
            this.db.Open();
        }

        #region ISession 成员

        public string Database
        {
            get;
            private set;
        }

        public IDbConnection Connection
        {
            get { return this.db.Connection; }
        }

        public ObjectPool Pool
        {
            get
            {
                return Pooler.Instance.GetPool(this.Database);
            }
        }

        #endregion

        #region IDisposable 成员

        public void Dispose()
        {
            this.db.Close();
            this.db = null;
        }

        #endregion
    }
}
