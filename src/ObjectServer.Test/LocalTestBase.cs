using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Dynamic;

using NUnit.Framework;

namespace ObjectServer
{
    public abstract class LocalTestCase
    {
        [TestFixtureSetUp()]
        public virtual void InitFramework()
        {
            if (!Platform.Initialized)
            {
                Platform.Initialize();
            }

            var service = Platform.ExportedService;
            this.Service = service;

            this.SessionId = this.Service.LogOn("objectserver", "root", "root");

            this.ActiveTestModule();
        }

        private void ActiveTestModule()
        {
            //激活 test 模块
            using (var scope = new ServiceScope(this.SessionId))
            {
                var constraints = new object[][] { new object[] { "name", "=", "test" } };
                var moduleModel = (ObjectServer.Model.IModel)scope.GetResource("core.module");
                var ids = moduleModel.SearchInternal(scope, constraints);
                dynamic fields = new ExpandoObject();
                fields.state = "activated";
                moduleModel.WriteInternal(scope, ids[0], fields);
            }
        }

        [TestFixtureTearDown]
        public virtual void DisposeFramework()
        {
            var service = Platform.ExportedService;
            ClearTestUsers(service);

            service.LogOff(this.SessionId);
        }

        private void ClearTestUsers(IExportedService service)
        {

            var constraints = new object[] { new object[] { "login", "=", "test" } };
            var ids = service.SearchModel(this.SessionId, "core.user", constraints);
            if (ids.Length > 0)
            {
                service.DeleteModel(this.SessionId, "core.user", ids.Select(i => (object)i).ToArray());
            }
        }

        public string SessionId { get; private set; }

        public IExportedService Service { get; private set; }

        public IServiceScope ServiceScope { get; private set; }

        [SetUp]
        public void BeforeTest()
        {
            Debug.Assert(this.ServiceScope == null);
            Debug.Assert(!string.IsNullOrEmpty(this.SessionId));

            this.ServiceScope = new ServiceScope(this.SessionId);
        }

        [TearDown]
        public void AfterTest()
        {
            Debug.Assert(this.ServiceScope != null);
            this.ServiceScope.Dispose();
            this.ServiceScope = null;
        }

        protected void ClearTestModelTable()
        {
            Debug.Assert(this.ServiceScope != null);
            this.ClearModel(this.ServiceScope, "test.test_model");
        }


        protected void ClearMasterAndChildTable()
        {
            Debug.Assert(this.ServiceScope != null);
            this.ClearModel(this.ServiceScope, "test.child");
            this.ClearModel(this.ServiceScope, "test.master");
        }

        protected void ClearManyToManyModels()
        {
            Debug.Assert(this.ServiceScope != null);
            this.ClearModel(this.ServiceScope, "test.department_employee");
            this.ClearModel(this.ServiceScope, "test.department");
            this.ClearModel(this.ServiceScope, "test.employee");
        }

        protected void ClearModel(IServiceScope scope, string model)
        {
            dynamic res = scope.GetResource(model);
            var ids = res.SearchInternal(scope);
            if (ids.Length > 0)
            {
                res.DeleteInternal(scope, ids);
            }

        }

    }
}
