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
        [Test]
        public void Test_create_and_delete_database()
        {
            var cfg = new Config();

            if (!Environment.Initialized)
            {
                Environment.Initialize(cfg);
            }

            var dbName = "os-testdb";
            var adminPassword = "root";
            var hashedRootPassword = ObjectServer.Utility.Sha.ToSha(Environment.Configuration.ServerPassword);

            var service = Environment.ExportedService;

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
