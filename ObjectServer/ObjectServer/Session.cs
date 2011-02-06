using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace ObjectServer
{
    internal class Session : ISession
    {

        public Session(IDbConnection conn)
        {
            this.Connection = conn;
        }

        #region ISession 成员

        public IDbConnection Connection
        {
            get;
            private set;
        }

        #endregion
    }
}
