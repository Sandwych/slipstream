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
            if (!ObjectServerStarter.Initialized)
            {
                ObjectServerStarter.Initialize();
            }

            var service = ObjectServerStarter.ExportedService;
            this.Service = service;

            this.SessionId = this.Service.LogOn("objectserver", "root", "root");

            this.ActiveTestModule();
        }

        private void ActiveTestModule()
        {
            //激活 test 模块
            using (var scope = new ServiceScope(new Guid(this.SessionId)))
            {
                var domain = new object[][] { new object[] { "name", "=", "test" } };
                var moduleModel = (ObjectServer.Model.IModel)scope.GetResource("core.module");
                var ids = moduleModel.SearchInternal(scope, domain);
                dynamic fields = new ExpandoObject();
                fields.state = "activated";
                moduleModel.WriteInternal(scope, ids[0], fields);
            }
        }

        [TestFixtureTearDown]
        public virtual void DisposeFramework()
        {
            var service = ObjectServerStarter.ExportedService;
            service.LogOff(this.SessionId);
        }

        public string SessionId { get; private set; }

        public IExportedService Service { get; private set; }

        public IServiceScope ResourceScope { get; private set; }

        [SetUp]
        public void BeforeTest()
        {
            Debug.Assert(this.ResourceScope == null);
            Debug.Assert(!string.IsNullOrEmpty(this.SessionId));

            this.ResourceScope = new ServiceScope(new Guid(this.SessionId));
        }

        [TearDown]
        public void AfterTest()
        {
            Debug.Assert(this.ResourceScope != null);
            this.ResourceScope.Dispose();
            this.ResourceScope = null;
        }

        protected void ClearTestModelTable()
        {
            Debug.Assert(this.ResourceScope != null);
            this.ClearModel(this.ResourceScope, "test.test_model");
        }


        protected void ClearMasterAndChildTable()
        {
            Debug.Assert(this.ResourceScope != null);
            this.ClearModel(this.ResourceScope, "test.child");
            this.ClearModel(this.ResourceScope, "test.master");
        }

        protected void ClearManyToManyModels()
        {
            Debug.Assert(this.ResourceScope != null);
            this.ClearModel(this.ResourceScope, "test.department_employee");
            this.ClearModel(this.ResourceScope, "test.department");
            this.ClearModel(this.ResourceScope, "test.employee");
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
