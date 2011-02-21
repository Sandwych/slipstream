using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

using ObjectServer.Backend;

namespace ObjectServer
{
    public abstract class ServiceObject : IServiceObject
    {
        private readonly Dictionary<string, MethodInfo> serviceMethods =
            new Dictionary<string, MethodInfo>();

        protected ServiceObject()
        {
            this.RegisterAllServiceMethods();
        }

        public MethodInfo GetServiceMethod(string name)
        {
            return this.serviceMethods[name];
        }

        /// <summary>
        /// 为动态语言预留的
        /// </summary>
        protected void RegisterServiceMethod(MethodInfo mi)
        {
            this.serviceMethods.Add(mi.Name, mi);
        }

        private void RegisterAllServiceMethods()
        {
            var t = this.GetType();
            var methods = t.GetMethods();
            foreach (var m in methods)
            {
                var attrs = m.GetCustomAttributes(typeof(ServiceMethodAttribute), false);
                if (attrs.Length > 0)
                {
                    this.RegisterServiceMethod(m);
                }
            }
        }


        public virtual void Initialize(IDatabaseContext db, ObjectPool pool)
        {
            this.Pool = pool;
        }

        public abstract string Name { get; protected set; }

        public abstract string Label { get; protected set; }

        public abstract bool DatabaseRequired { get; }

        public abstract string[] ReferencedObjects { get; }

        public ObjectPool Pool { get; private set; }

    }
}
