using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Dynamic;
using System.Diagnostics;

using ObjectServer.Exceptions;
using ObjectServer.Data;

namespace ObjectServer
{
    /// <summary>
    /// 适用于静态语言（非DLR）的服务对象基类
    /// </summary>
    public abstract class AbstractResource : DynamicObject, IResource
    {
        private readonly IDictionary<string, ITransaction> services =
            new Dictionary<string, ITransaction>();

        protected AbstractResource(string name)
        {
            Debug.Assert(!string.IsNullOrEmpty(name));

            LoggerProvider.EnvironmentLogger.Debug(() => string.Format("Registering Resource: [{0}]", name));
            this.SetName(name);
            this.RegisterAllServiceMethods(this.GetType());
        }

        private void SetName(string name)
        {
            if (!NamingRule.IsValidResourceName(name))
            {
                var msg = string.Format("Invalid service object name: '{0}'", name);
                LoggerProvider.EnvironmentLogger.Error(() => msg);
                throw new ResourceException(msg);
            }

            this.Name = name;

            if (string.IsNullOrEmpty(this.Label))
            {
                this.Label = name;
            }
        }

        public ITransaction GetService(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }

            ITransaction service;
            if (!this.services.TryGetValue(name, out service))
            {
                var msg = String.Format("Cannot found service: [{0}]", name);
                throw new ArgumentOutOfRangeException("name", msg);
            }

            return service;
        }

        /// <summary>
        /// 只能在进程内调用的方法
        /// </summary>
        /// <param name="binder"></param>
        /// <param name="args"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            result = null;
            ITransaction service;

            Debug.Assert(args.Length > 0);
            var scope = (ITransactionContext)args[0];
            var userArgs = new object[args.Length - 1];
            Array.Copy(args, 1, userArgs, 0, args.Length - 1);

            if (this.services.TryGetValue(binder.Name, out service))
            {
                result = service.Invoke(this, scope, userArgs);
                return true;
            }
            else
            {
                return base.TryInvokeMember(binder, args, out result);
            }
        }

        /// <summary>
        /// 为动态语言预留的
        /// </summary>
        protected void RegisterServiceMethod(string name, MethodInfo mi)
        {
            Debug.Assert(mi != null);
            Debug.Assert(!string.IsNullOrEmpty(name));

            this.VerifyMethod(mi);
            var clrSvc = new ClrTransaction(this, name, mi);
            this.services.Add(clrSvc.Name, clrSvc);
        }

        private void VerifyMethod(MethodInfo mi)
        {
            Debug.Assert(mi != null);

            if (!mi.IsStatic)
            {
                var msg = string.Format(
                    "Service method '{1}' of resource '{0}' must be a static method",
                    this.Name, mi.Name);
                LoggerProvider.EnvironmentLogger.Error(() => msg);
                throw new BadServiceMethodException(msg, this.Name, mi.Name);
            }

            var parameters = mi.GetParameters();
            if (parameters.Length < 2
                || parameters[1].ParameterType != typeof(ITransactionContext)
                || !mi.IsPublic)
            {
                var msg = string.Format(
                    "The method [{1}] of resource [{0}] must have an IContext parameter at second position.",
                    this.Name, mi.Name);
                LoggerProvider.EnvironmentLogger.Error(() => msg);
                throw new BadServiceMethodException(msg, this.Name, mi.Name);
            }
        }

        protected void RegisterAllServiceMethods(Type resourceType)
        {
            if (resourceType == null)
            {
                throw new ArgumentNullException("resourceType");
            }

            var methods = resourceType.GetMethods().Where(m => m.IsStatic && m.ReflectedType == resourceType);
            foreach (var m in methods)
            {
                var attr = Attribute.GetCustomAttribute(
                    m, typeof(TransactionMethodAttribute), false) as TransactionMethodAttribute;
                if (attr != null)
                {
                    this.RegisterServiceMethod(attr.Name, m);
                }
            }
        }

        public virtual void Initialize(ITransactionContext tc, bool update)
        {
            if (tc == null)
            {
                throw new ArgumentNullException("db");
            }

            LoggerProvider.EnvironmentLogger.Info(
                () => String.Format("Initializing resource [{0}]", this.Name));

            //确保加载资源之前设置了合适的属性
            Debug.Assert(!string.IsNullOrEmpty(this.Name));
            Debug.Assert(!string.IsNullOrEmpty(this.Module));
        }

        public string Name { get; private set; }

        public string Label { get; protected set; }

        public int DependencyWeight { get; private set; }

        /// <summary>
        /// 属性由载入器负责设置
        /// </summary>
        public string Module { get; internal set; }

        public abstract string[] GetReferencedObjects();

        public ICollection<ITransaction> Services
        {
            get { return this.services.Values; }
        }

        #region ServiceObject(s) factory methods

        internal static AbstractResource CreateStaticResourceInstance(Type t)
        {
            Debug.Assert(t != null);

            AbstractResource obj;

            try
            {
                obj = Activator.CreateInstance(t) as AbstractResource;
            }
            catch (TargetInvocationException ex)
            {
                throw ex.InnerException;
            }

            if (obj == null)
            {
                var msg = string.Format("类型 '{0}' 没有实现 IResource 接口", t.FullName);
                throw new InvalidCastException(msg);
            }
            return obj;
        }

        //以后要支持 DLR，增加  CreateDynamicObjectInstance

        #endregion

        public virtual void MergeFrom(IResource res)
        {
            if (res == null)
            {
                throw new ArgumentNullException("res");
            }

            foreach (var p in res.Services)
            {
                this.services[p.Name] = p;
            }
        }

    }
}
