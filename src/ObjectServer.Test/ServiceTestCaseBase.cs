using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Dynamic;

using NUnit.Framework;

namespace ObjectServer
{
    public abstract class ServiceTestCaseBase
    {
        public const string TestingDatabaseName = "osdb_test";

        [TestFixtureSetUp()]
        public virtual void InitFramework()
        {
            var cfg = new Config();
            cfg.DbName = TestingDatabaseName;

            if (!SlipstreamEnvironment.Initialized)
            {
                SlipstreamEnvironment.Initialize(cfg);
            }

            var service = SlipstreamEnvironment.RootService;

            var dbs = SlipstreamEnvironment.RootService.ListDatabases();
            if (!dbs.Contains(TestingDatabaseName))
            {
                var hashedRootPassword = ObjectServer.Utility.Sha.ToSha(
                    SlipstreamEnvironment.Configuration.ServerPassword);
                SlipstreamEnvironment.RootService.CreateDatabase(hashedRootPassword, TestingDatabaseName, "root");
            }

            this.SessionToken = service.LogOn(TestingDatabaseName, "root", "root");
        }

        [TestFixtureTearDown]
        public virtual void DisposeFramework()
        {
            var service = SlipstreamEnvironment.RootService;
            service.LogOff(TestingDatabaseName, this.SessionToken);
        }

        public string SessionToken { get; private set; }
    }
}
