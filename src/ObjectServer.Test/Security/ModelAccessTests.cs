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
            var service = ObjectServer.Platform.ExportedService;
            var sessionId = service.LogOn("objectserver", "testuser", "testuser");

            using (var scope = new ServiceScope(sessionId))
            {
                var userModel = (ObjectServer.Model.IModel)scope.GetResource("core.user");

                Assert.DoesNotThrow(() =>
                {
                    var ids = userModel.SearchInternal(scope);
                    Assert.True(ids.Length > 0);
                    userModel.ReadInternal(scope, ids);
                });

                Assert.Throws<UnauthorizedAccessException>(() =>
                {
                    dynamic record = new ExpandoObject();
                    userModel.CreateInternal(scope, record);
                });

                Assert.Throws<UnauthorizedAccessException>(() =>
                {
                    dynamic record = new ExpandoObject();
                    userModel.WriteInternal(scope, 1, record);
                });

                Assert.Throws<UnauthorizedAccessException>(() =>
                {
                    userModel.DeleteInternal(scope, new long[] { 1 });
                });

            }

            service.LogOff(sessionId);

        }
    }
}
