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
        }

        [TestFixtureTearDown]
        public virtual void DisposeFramework()
        {
            var service = Environment.ExportedService;
            service.LogOff(TestingDatabaseName, this.SessionId);
        }

        public string SessionId { get; private set; }
    }
}
