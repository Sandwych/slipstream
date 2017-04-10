using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;

using Autofac;
using NUnit.Framework;

namespace SlipStream.Entity
{
    [TestFixture(Category = "Security")]
    public class EntityAccessTests : ServiceTestCaseBase
    {
        [Test]
        public void ExpectAccessDenied()
        {
            var service = SlipStream.SlipstreamEnvironment.RootService;
            var sessionToken = service.LogOn(TestingDatabaseName, "testuser", "testuser");
            var dataProvider = SlipstreamEnvironment.RootContainer.Resolve<Data.IDataProvider>();
            var dbDomain = SlipstreamEnvironment.DbDomains.GetDbDomain(TestingDatabaseName);
            using (var ctx = dbDomain.OpenSession(sessionToken))
            {
                dynamic userEntity = dbDomain.GetResource(Core.UserEntity.EntityName);

                Assert.DoesNotThrow(() =>
                {
                    var ids = userEntity.Search(null, null, 0, 0);
                    Assert.True(ids.Length > 0);
                    userEntity.Read(ids, null);
                });

                Assert.Throws<SlipStream.Exceptions.SecurityException>(() =>
                {
                    dynamic record = new ExpandoObject();
                    record.login = "login";
                    userEntity.Create(record);
                });

                Assert.Throws<SlipStream.Exceptions.SecurityException>(() =>
                {
                    dynamic record = new ExpandoObject();
                    record.login = "login";
                    userEntity.Write((long)1, record);
                });

                Assert.Throws<SlipStream.Exceptions.SecurityException>(() =>
                {
                    userEntity.Delete(new long[] { (long)1 });
                });

            }

            service.LogOff(TestingDatabaseName, sessionToken);

        }
    }
}
