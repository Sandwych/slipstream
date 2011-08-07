using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using ObjectServer.Data;

namespace ObjectServer
{
    internal class InternalServiceScope : IServiceScope
    {
        public InternalServiceScope(IDBProfile db, Session session)
        {
            Debug.Assert(db != null);
            Debug.Assert(session != null);
            this.DBProfile = db;
            this.Session = session;
        }

        private IDBProfile DBProfile { get; set; }

        public Session Session { get; private set; }

        public void Dispose()
        {
        }

        public bool Equals(IServiceScope other)
        {
            throw new NotSupportedException("Invalid Equals invocation");
        }

        public IResource GetResource(string resName)
        {
            if (string.IsNullOrEmpty(resName))
            {
                throw new ArgumentNullException("resName");
            }

            return this.DBProfile.GetResource(resName);
        }

        public IDBContext DBContext
        {
            get
            {
                Debug.Assert(this.DBProfile != null);
                return this.DBProfile.DBContext;
            }
        }
    }
}
