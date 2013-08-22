using Autofac;
using NUnit.Framework;

namespace SlipStream.Test
{
    public class ContainerTestBase
    {
        protected IContainer _container;

        [SetUp]
        public virtual void Init()
        {
            var builder = new ContainerBuilder();            
            Register(builder);
            this._container = builder.Build();
            Resolve(this._container);
        }

        [TearDown]
        public virtual void Cleanup()
        {
            if (this._container != null)
            {
                this._container.Dispose();
            }
        }

        protected virtual void Register(ContainerBuilder builder) { }
        protected virtual void Resolve(ILifetimeScope container) { }
    }
}