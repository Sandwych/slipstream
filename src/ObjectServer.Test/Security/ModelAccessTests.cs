using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;

using Autofac;
using NUnit.Framework;

namespace ObjectServer.Model.Test
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

            using (var ctx = new ServiceContext(dataProvider, TestingDatabaseName, sessionToken))
            {
                dynamic userModel = ctx.GetResource("core.user");

                Assert.DoesNotThrow(() =>
                {
                    var ids = userModel.Search(ctx, null, null, 0, 0);
                    Assert.True(ids.Length > 0);
                    userModel.Read(ctx, ids, null);
                });

                Assert.Throws<ObjectServer.Exceptions.SecurityException>(() =>
                {
                    dynamic record = new ExpandoObject();
                    record.login = "login";
                    userModel.Create(ctx, record);
                });

                Assert.Throws<ObjectServer.Exceptions.SecurityException>(() =>
                {
                    dynamic record = new ExpandoObject();
                    record.login = "login";
                    userModel.Write(ctx, (long)1, record);
                });

                Assert.Throws<ObjectServer.Exceptions.SecurityException>(() =>
                {
                    userModel.Delete(ctx, new long[] { (long)1 });
                });

            }

            service.LogOff(TestingDatabaseName, sessionToken);

        }
    }
}
