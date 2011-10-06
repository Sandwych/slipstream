using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;

using NUnit.Framework;

namespace ObjectServer.Model.Test
{
    [TestFixture]
    public class ModelAccessTests : TransactionContextTestCaseBase
    {
        [Test]
        public void ExpectAccessDenied()
        {
            var service = ObjectServer.Environment.ExportedService;
            var sessionId = service.LogOn(TestingDatabaseName, "testuser", "testuser");

            using (var scope = new TransactionContext(TestingDatabaseName, sessionId))
            {
                var userModel = this.GetResource("core.user");

                Assert.DoesNotThrow(() =>
                {
                    var ids = userModel.SearchInternal(scope, null, null, 0, 0);
                    Assert.True(ids.Length > 0);
                    userModel.Read(scope, ids, null);
                });

                Assert.Throws<ObjectServer.Exceptions.SecurityException>(() =>
                {
                    dynamic record = new ExpandoObject();
                    record.login = "login";
                    userModel.Create(scope, record);
                });

                Assert.Throws<ObjectServer.Exceptions.SecurityException>(() =>
                {
                    dynamic record = new ExpandoObject();
                    record.login = "login";
                    userModel.Write(scope, (long)1, record);
                });

                Assert.Throws<ObjectServer.Exceptions.SecurityException>(() =>
                {
                    userModel.Delete(scope, new long[] { (long)1 });
                });

            }

            service.LogOff(TestingDatabaseName, sessionId);

        }
    }
}
