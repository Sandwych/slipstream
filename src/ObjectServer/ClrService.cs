using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Diagnostics;

namespace ObjectServer
{
    internal class ClrService : IService
    {
        public ClrService(IResource res, string name, MethodInfo mi)
        {
            Debug.Assert(!string.IsNullOrEmpty(name));
            Debug.Assert(mi != null);

            this.Name = name;
            this.Method = mi;
            this.Resource = res;
        }

        #region IService Members

        public object Invoke(params object[] parameters)
        {
            return this.Method.Invoke(this.Resource, parameters);
        }

        public IResource Resource { get; private set; }

        public string Name { get; private set; }

        public string Help { get; set; }

        #endregion

        public MethodInfo Method { get; private set; }
    }
}
