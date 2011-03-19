using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace ObjectServer
{
    internal class InternalResourceScope : IResourceScope
    {
        public InternalResourceScope(IDatabaseProfile db, Session session)
        {
            Debug.Assert(db != null);
            Debug.Assert(session != null);
            this.DatabaseProfile = db;
            this.Session = session;
        }

        public IDatabaseProfile DatabaseProfile { get; private set; }

        public Session Session { get; private set; }

        public void Dispose()
        {
        }

        public bool Equals(IResourceScope other)
        {
            throw new NotSupportedException("Invalid Equals invocation");
        }

        public IResource GetResource(string resName)
        {
            if (string.IsNullOrEmpty(resName))
            {
                throw new ArgumentNullException("resName");
            }

            return this.DatabaseProfile.GetResource(resName);
        }
    }
}
