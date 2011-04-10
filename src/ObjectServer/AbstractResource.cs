using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Dynamic;
using System.Diagnostics;

using ObjectServer.Backend;

namespace ObjectServer
{
    /// <summary>
    /// 适用于静态语言（非DLR）的服务对象基类
    /// </summary>
    public abstract class AbstractResource : DynamicObject, IResource
    {
        private readonly IDictionary<string, MethodInfo> serviceMethods =
            new Dictionary<string, MethodInfo>();

        protected AbstractResource(string name)
        {
            this.SetName(name);
            this.RegisterAllServiceMethods(this.GetType());
        }

        private void SetName(string name)
        {
            if (!NamingRule.IsValidResourceName(name))
            {
                var msg = string.Format("Invalid service object name: '{0}'", name);
                Logger.Error(() => msg);
                throw new BadResourceNameException(msg, name);
            }

            this.Name = name;

            if (string.IsNullOrEmpty(this.Label))
            {
                this.Label = name;
            }
        }

        public MethodInfo GetServiceMethod(string name)
        {
            return this.serviceMethods[name];
        }

        public override bool TryInvokeMember(System.Dynamic.InvokeMemberBinder binder, object[] args, out object result)
        {
            result = null;
            MethodInfo methodInfo;
            var allArgs = new object[args.Length + 1];
            allArgs[0] = this;
            args.CopyTo(allArgs, 1);

            if (this.serviceMethods.TryGetValue(binder.Name, out methodInfo))
            {
                result = methodInfo.Invoke(this, allArgs);
                return true;
            }
            else
            {
                return base.TryInvokeMember(binder, allArgs, out result);
            }
        }

        /// <summary>
        /// 为动态语言预留的
        /// </summary>
        protected void RegisterServiceMethod(MethodInfo mi)
        {
            //TODO: 再好好地思考一下模型的继承问题
            this.VerifyMethod(mi);
            this.serviceMethods.Add(mi.Name, mi);
        }

        private void VerifyMethod(MethodInfo mi)
        {
            if (!mi.IsStatic)
            {
                var msg = string.Format(
                    "Service method '{1}' of resource '{0}' must be a static method",
                    this.Name, mi.Name);
                Logger.Error(() => msg);
                throw new BadServiceMethodException(msg, this.Name, mi.Name);
            }

            var parameters = mi.GetParameters();
            if (parameters.Length < 2
                || parameters[1].ParameterType != typeof(IServiceScope)
                || !mi.IsPublic)
            {
                var msg = string.Format(
                    "The method '{1}' of resource {0} must have an IContext parameter at second position.",
                    this.Name, mi.Name);
                Logger.Error(() => msg);
                throw new BadServiceMethodException(msg, this.Name, mi.Name);
            }
        }

        private void RegisterAllServiceMethods(Type t)
        {
            Debug.Assert(t != null);

            var methods = t.GetMethods().Where(m => m.IsStatic && m.ReflectedType == t);
            foreach (var m in methods)
            {
                var attr = Attribute.GetCustomAttribute(m, typeof(ServiceMethodAttribute));
                if (attr != null)
                {
                    this.RegisterServiceMethod(m);
                }
            }
        }

        public virtual void Load(IDBProfile db)
        {
            //确保加载资源之前设置了合适的属性
            Debug.Assert(!string.IsNullOrEmpty(this.Name));
            Debug.Assert(!string.IsNullOrEmpty(this.Module));
        }

        public string Name { get; private set; }

        public string Label { get; protected set; }

        /// <summary>
        /// 属性由载入器负责设置
        /// </summary>
        public string Module { get; internal set; }

        public abstract bool DatabaseRequired { get; }

        public abstract string[] GetReferencedObjects();

        public ICollection<MethodInfo> ServiceMethods
        {
            get { return this.serviceMethods.Values; }
        }

        #region ServiceObject(s) factory methods

        internal static AbstractResource CreateStaticResourceInstance(Type t)
        {
            var obj = Activator.CreateInstance(t) as AbstractResource;
            if (obj == null)
            {
                var msg = string.Format("类型 '{0}' 没有实现 IServiceObject 接口", t.FullName);
                throw new InvalidCastException(msg);
            }
            return obj;
        }

        internal static T CreateStaticObjectInstance<T>()
            where T : class, IResource
        {
            return CreateStaticResourceInstance(typeof(T)) as T;
        }

        //以后要支持 DLR，增加  CreateDynamicObjectInstance

        #endregion

        public virtual void MergeFrom(IResource res)
        {

            foreach (var p in res.ServiceMethods)
            {
                this.serviceMethods[p.Name] = p;
            }
        }

    }
}
