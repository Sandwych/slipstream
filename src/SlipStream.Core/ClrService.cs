using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Diagnostics;

namespace SlipStream
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

        public object Invoke(IResource self, params object[] parameters)
        {
            if (self == null)
            {
                throw new ArgumentNullException("self");
            }
            var userParamCount = parameters == null ? 0 : parameters.Length;
            var args = new object[userParamCount + 1];
            args[0] = self;
            parameters.CopyTo(args, 1);

            try
            {
                return this.Method.Invoke(null, args);
            }
            catch (TargetInvocationException tiex)
            {
                throw tiex.InnerException;
            }
            catch
            {
                throw;
            }
        }

        public IResource Resource { get; private set; }

        public string Name { get; private set; }

        public string Help { get; set; }

        #endregion

        public MethodInfo Method { get; private set; }
    }
}
