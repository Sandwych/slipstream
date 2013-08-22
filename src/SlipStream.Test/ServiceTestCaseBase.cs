using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Dynamic;
using System.Reflection;
using System.IO;

using Sandwych.Utility;
using Sandwych;
using NUnit.Framework;

namespace SlipStream
{
    public abstract class ServiceTestCaseBase
    {
        public const string TestingDatabaseName = "slipstream_testdb";

        [TestFixtureSetUp()]
        public virtual void InitFramework()
        {
            var cfg = new ShellSettings();
            cfg.DbName = TestingDatabaseName;
            cfg.ModulePath = Path.Combine(Environment.CurrentDirectory, "Modules");

            if (!SlipstreamEnvironment.Initialized)
            {
                SlipstreamEnvironment.Initialize(cfg);
            }

            var service = SlipstreamEnvironment.RootService;

            var dbs = SlipstreamEnvironment.RootService.ListDatabases();
            if (!dbs.Contains(TestingDatabaseName))
            {
                var hashedRootPassword = SlipstreamEnvironment.Settings.ServerPassword.ToSha();
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
