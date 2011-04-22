using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;

namespace ObjectServer.Core.Test
{

    [TestFixture]
    public sealed class UserTest : LocalTestCase
    {
        [Test]
        public void Test_user_password_hashing()
        {
            var userRecord = new Dictionary<string, object>()
            {
                { "name", "测试用户" },
                { "login", "test" },
                { "password", "test" },
                { "admin", false },
            };

            var uid = this.Service.CreateModel(this.SessionId, UserModel.ModelName, userRecord);
            var records = this.Service.ReadModel(this.SessionId, UserModel.ModelName, new object[] { uid }, null);
            var user1 = records[0];

            var salt = (string)user1["salt"];
            Assert.IsNull(salt);
        }

    }
}
