using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using ObjectServer.Backend;

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

        public IDBProfile DBProfile { get; private set; }

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

        public IDBConnection Connection
        {
            get
            {
                Debug.Assert(this.DBProfile != null);
                return this.DBProfile.Connection;
            }
        }
    }
}
