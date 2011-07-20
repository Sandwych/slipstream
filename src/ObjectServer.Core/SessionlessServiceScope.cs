using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using ObjectServer.Data;

namespace ObjectServer
{
    internal class SessionlessServiceScope : IServiceScope
    {
        public SessionlessServiceScope(IDBProfile db)
        {
            Debug.Assert(db != null);
            this.DBProfile = db;
        }

        private IDBProfile DBProfile { get; set; }

        public Session Session
        {
            get
            {
                throw new NotSupportedException();
            }
        }

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
