using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;

namespace ObjectServer.Core.Test
{

    [TestFixture]
    public sealed class UserTest : ServiceContextTestCaseBase
    {
        [Test]
        public void CanNotGetPasswordAndSalt()
        {
            dynamic userModel = this.GetResource("core.user");
            var searchDomain = new object[][] { new object[] { "login", "=", "test" } };
            var ids = userModel.Search(this.Context, searchDomain, null, 0, 0);
            if (ids.Length > 0)
            {
                userModel.Delete(this.Context, ids);
            }

            var userRecord = new Dictionary<string, object>()
            {
                { "name", "Testing User" },
                { "login", "test" },
                { "password", "test" },
                { "admin", false },
            };

            var fields = new string[] { "name", "login", "password", "salt" };

            var uid = userModel.Create(this.Context, userRecord);
            dynamic records = userModel.Read(this.Context, new object[] { uid }, fields);
            var user1 = records[0];
            var salt = (string)user1["salt"];
            Assert.IsNull(salt);
            Assert.AreNotEqual(userRecord["password"], user1["password"]);
        }

    }
}
