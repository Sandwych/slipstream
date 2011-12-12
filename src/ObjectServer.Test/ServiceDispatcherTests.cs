using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;

using NUnit.Framework;

namespace ObjectServer.Test
{
    [TestFixture]
    public class ServiceDispatcherTests
    {
        [Ignore]
        public void CreateAndDeleteDatabaseShouldBeSuccessfully()
        {
            var cfg = new Config();

            if (!SlipstreamEnvironment.Initialized)
            {
                SlipstreamEnvironment.Initialize(cfg);
            }

            var dbName = "os-testdb";
            var adminPassword = "root";
            var hashedRootPassword = ObjectServer.Utility.Sha.ToSha(SlipstreamEnvironment.Configuration.ServerPassword);

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
