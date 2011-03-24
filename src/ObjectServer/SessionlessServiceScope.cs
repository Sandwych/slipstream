using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace ObjectServer
{
    internal class SessionlessServiceScope : IServiceScope
    {
        public SessionlessServiceScope(IDatabaseProfile db)
        {
            Debug.Assert(db != null);
            this.DatabaseProfile = db;
        }

        public IDatabaseProfile DatabaseProfile { get; private set; }

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

            return this.DatabaseProfile.GetResource(resName);
        }
    }
}
