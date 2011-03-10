using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace ObjectServer
{
    internal class InternalResourceScope : IResourceScope
    {
        public InternalResourceScope(IDatabase db, Session session)
        {
            Debug.Assert(db != null);
            Debug.Assert(session != null);
            this.Database = db;
            this.Session = session;
        }

        public IDatabase Database { get; private set; }

        public Session Session { get; private set; }

        public void Dispose()
        {
        }

        public bool Equals(IResourceScope other)
        {
            throw new NotSupportedException("Invalid Equals invocation");
        }
    }
}
