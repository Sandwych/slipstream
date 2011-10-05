using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Dynamic;

using NUnit.Framework;

namespace ObjectServer
{
    public abstract class TransactionTestCaseBase
    {
        public const string TestingDatabaseName = "osdb_test";

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

            var dbs = Environment.ExportedService.ListDatabases();
            if (!dbs.Contains(TestingDatabaseName))
            {
                var hashedRootPassword = ObjectServer.Utility.Sha.ToSha(
                    Environment.Configuration.ServerPassword);
                Environment.ExportedService.CreateDatabase(hashedRootPassword, TestingDatabaseName, "root");
            }

            this.SessionId = service.LogOn(TestingDatabaseName, "root", "root");
            this.ActiveTestModule();
        }

        private void ActiveTestModule()
        {
            //激活 test 模块
            using (var scope = new TransactionContext(TestingDatabaseName, this.SessionId))
            {
                var constraints = new object[][] { new object[] { "name", "=", "test" } };
                var moduleModel = (ObjectServer.Model.IModel)scope.GetResource("core.module");
                var ids = moduleModel.SearchInternal(scope, constraints, null, 0, 0);
                dynamic fields = new ExpandoObject();
                fields.state = Core.ModuleModel.States.Installed;
                moduleModel.WriteInternal(scope, ids[0], fields);
            }
        }

        [TestFixtureTearDown]
        public virtual void DisposeFramework()
        {
            var service = Environment.ExportedService;
            service.LogOff(TestingDatabaseName, this.SessionId);
            Environment.Shutdown();
        }

        public string SessionId { get; private set; }
    }
}
