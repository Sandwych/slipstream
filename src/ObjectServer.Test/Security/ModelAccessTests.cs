using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;

using Autofac;
using NUnit.Framework;

namespace ObjectServer.Model
{
    [TestFixture]
    public class ModelAccessTests : ServiceTestCaseBase
    {
        [Test]
        public void ExpectAccessDenied()
        {
            var service = ObjectServer.SlipstreamEnvironment.RootService;
            var sessionToken = service.LogOn(TestingDatabaseName, "testuser", "testuser");
            var dataProvider = SlipstreamEnvironment.RootContainer.Resolve<Data.IDataProvider>();
            var dbDomain = SlipstreamEnvironment.DbDomains.GetDbDomain(TestingDatabaseName);
            using (var ctx = dbDomain.OpenSession(sessionToken))
            {
                dynamic userModel = dbDomain.GetResource("core.user");

                Assert.DoesNotThrow(() =>
                {
                    var ids = userModel.Search(null, null, 0, 0);
                    Assert.True(ids.Length > 0);
                    userModel.Read(ids, null);
                });

                Assert.Throws<ObjectServer.Exceptions.SecurityException>(() =>
                {
                    dynamic record = new ExpandoObject();
                    record.login = "login";
                    userModel.Create(record);
                });

                Assert.Throws<ObjectServer.Exceptions.SecurityException>(() =>
                {
                    dynamic record = new ExpandoObject();
                    record.login = "login";
                    userModel.Write((long)1, record);
                });

                Assert.Throws<ObjectServer.Exceptions.SecurityException>(() =>
                {
                    userModel.Delete(new long[] { (long)1 });
                });

            }

            service.LogOff(TestingDatabaseName, sessionToken);

        }
    }
}
