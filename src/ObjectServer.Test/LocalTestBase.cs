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
        public const string TestingDatabaseName = "objectserver";

        [TestFixtureSetUp()]
        public virtual void InitFramework()
        {
            var cfg = new Config();
            cfg.DbName = TestingDatabaseName;

            if (!Environment.Initialized)
            {
                Environment.Initialize(cfg);
            }

            var service = Environment.ExportedService;
            this.Service = service;

            var dbs = Environment.ExportedService.ListDatabases();
            if (!dbs.Contains(TestingDatabaseName))
            {
                var hashedRootPassword = ObjectServer.Utility.Sha.ToSha(
                    Environment.Configuration.ServerPassword);
                Environment.ExportedService.CreateDatabase(hashedRootPassword, TestingDatabaseName, "root");
            }


            this.SessionId = this.Service.LogOn(TestingDatabaseName, "root", "root");

            this.ActiveTestModule();
        }

        private void ActiveTestModule()
        {
            //激活 test 模块
            using (var scope = new TransactionContext(this.SessionId))
            {
                var constraints = new object[][] { new object[] { "name", "=", "test" } };
                var moduleModel = (ObjectServer.Model.IModel)scope.GetResource("core.module");
                var ids = moduleModel.SearchInternal(scope, constraints);
                dynamic fields = new ExpandoObject();
                fields.state = Core.ModuleModel.States.Installed;
                moduleModel.WriteInternal(scope, ids[0], fields);
            }
        }

        [TestFixtureTearDown]
        public virtual void DisposeFramework()
        {
            var service = Environment.ExportedService;
            ClearTestUsers(service);

            service.LogOff(TestingDatabaseName, this.SessionId);
        }

        private void ClearTestUsers(IExportedService service)
        {

            var constraints = new object[] { new object[] { "login", "=", "test" } };
            var ids = service.SearchModel(TestingDatabaseName, this.SessionId, "core.user", constraints);
            if (ids.Length > 0)
            {
                service.DeleteModel(TestingDatabaseName, this.SessionId, "core.user", ids.Select(i => (object)i).ToArray());
            }
        }

        public string SessionId { get; private set; }

        public IExportedService Service { get; private set; }

        public IServiceContext ServiceContext { get; private set; }

        [SetUp]
        public void BeforeTest()
        {
            Debug.Assert(this.ServiceContext == null);
            Debug.Assert(!string.IsNullOrEmpty(this.SessionId));

            this.ServiceContext = new TransactionContext(this.SessionId);
        }

        [TearDown]
        public void AfterTest()
        {
            Debug.Assert(this.ServiceContext != null);
            this.ServiceContext.Dispose();
            this.ServiceContext = null;
        }

        protected void ClearTestModelTable()
        {
            Debug.Assert(this.ServiceContext != null);
            this.ClearModel("test.test_model");
        }


        protected void ClearMasterAndChildTable()
        {
            Debug.Assert(this.ServiceContext != null);
            this.ClearModel("test.child");
            this.ClearModel("test.master");
        }

        protected void ClearManyToManyModels()
        {
            Debug.Assert(this.ServiceContext != null);
            this.ClearModel("test.department_employee");
            this.ClearModel("test.department");
            this.ClearModel("test.employee");
        }

        protected void ClearModel(string model)
        {
            var ids = (long[])this.Service.Execute(this.SessionId, model, "Search", null, null, 0, 0);
            if (ids.Length > 0)
            {
                var idsToDel = ids.Select(e => (object)e).ToArray();
                this.Service.Execute(TestingDatabaseName, this.SessionId, model, "Delete", new object[] { idsToDel });
            }

        }

    }
}
