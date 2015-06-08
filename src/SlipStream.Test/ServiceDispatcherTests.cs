using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;
using System.IO;

using Sandwych.Utility;
using NUnit.Framework;

namespace SlipStream.Test
{
    [TestFixture]
    public class ServiceDispatcherTests
    {
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
