using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;

using NUnit.Framework;

namespace ObjectServer.Model.Test
{
    [TestFixture]
    public class ModelAccessTests : LocalTestCase
    {
        [Test]
        public void ExpectAccessDenied()
        {
            var service = ObjectServer.Environment.ExportedService;
            var sessionId = service.LogOn(TestingDatabaseName, "testuser", "testuser");

            using (var scope = new ServiceContext(sessionId))
            {
                var userModel = (ObjectServer.Model.IModel)scope.GetResource("core.user");

                Assert.DoesNotThrow(() =>
                {
                    var ids = userModel.SearchInternal(scope);
                    Assert.True(ids.Length > 0);
                    userModel.ReadInternal(scope, ids);
                });

                Assert.Throws<ObjectServer.Exceptions.SecurityException>(() =>
                {
                    dynamic record = new ExpandoObject();
                    record.login = "login";
                    userModel.CreateInternal(scope, record);
                });

                Assert.Throws<ObjectServer.Exceptions.SecurityException>(() =>
                {
                    dynamic record = new ExpandoObject();
                    record.login = "login";
                    userModel.WriteInternal(scope, 1, record);
                });

                Assert.Throws<ObjectServer.Exceptions.SecurityException>(() =>
                {
                    userModel.DeleteInternal(scope, new long[] { 1 });
                });

            }

            service.LogOff(sessionId);

        }
    }
}
