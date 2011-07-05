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
        [Ignore]
        public void ExpectAccessDenied()
        {
            //添加普通用户组用户 testuser
            dynamic testUser = new ExpandoObject();
            testUser.name = "testuser";
            testUser.login = "testuser";
            testUser.password = "testuser";
            testUser.admin = false;
            this.Service.CreateModel(this.SessionId, "core.user", testUser);
            

            var service = ObjectServer.Platform.ExportedService;
            var sessionId = service.LogOn("objectserver", "testuser", "testuser");

            using (var scope = new ServiceScope(new Guid(sessionId)))
            {
            }

            service.LogOff(sessionId);

        }
    }
}
