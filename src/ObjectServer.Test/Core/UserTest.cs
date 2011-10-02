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

            var uid = this.Service.Execute(TestingDatabaseName, this.SessionId, UserModel.ModelName, "Create", userRecord);
            dynamic records = this.Service.Execute(TestingDatabaseName, 
                this.SessionId, UserModel.ModelName, "Read", new object[] { uid }, null);            
            var user1 = records[0];

            var salt = (string)user1["salt"];
            Assert.IsNull(salt);
        }

    }
}
