using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace ObjectServer
{
    internal class SessionlessContext : IResourceScope
    {
        public SessionlessContext(IDatabase db)
        {
            Debug.Assert(db != null);
            this.Database = db;
        }

        public IDatabase Database { get; private set; }

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

        public bool Equals(IResourceScope other)
        {
            throw new NotSupportedException("Invalid Equals invocation");
        }
    }
}
