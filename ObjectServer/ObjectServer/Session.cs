using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace ObjectServer
{
    internal class Session : ISession
    {

        public Session(string db, IDbConnection conn)
        {
            this.Database = db;
            this.Connection = conn;
        }

        #region ISession 成员

        public string Database
        {
            get;
            private set;
        }

        public IDbConnection Connection
        {
            get;
            private set;
        }

        public ObjectPool Pool
        {
            get
            {
                return Pooler.Instance.GetPool(this.Database);
            }
        }

        #endregion
    }
}
