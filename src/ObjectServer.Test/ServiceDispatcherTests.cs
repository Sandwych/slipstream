using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;

using Malt.Utility;
using NUnit.Framework;

namespace ObjectServer.Test
{
    [TestFixture]
    public class ServiceDispatcherTests
    {
        [Ignore]
        public void CreateAndDeleteDatabaseShouldBeSuccessfully()
        {
            var cfg = new ShellSettings();

            if (!SlipstreamEnvironment.Initialized)
            {
                SlipstreamEnvironment.Initialize(cfg);
            }

            var dbName = "os-testdb";
            var adminPassword = "root";
            var hashedRootPassword = SlipstreamEnvironment.Settings.ServerPassword.ToSha();

            var service = SlipstreamEnvironment.RootService;

            Assert.DoesNotThrow(() =>
            {
                service.CreateDatabase(hashedRootPassword, dbName, adminPassword);
            });

            Assert.DoesNotThrow(() =>
            {
                service.DeleteDatabase(hashedRootPassword, dbName);
            });
        }

    }
}
