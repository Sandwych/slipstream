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
                { "name", "Testing User" },
                { "login", "test" },
                { "password", "test" },
                { "admin", false },
            };

            dynamic userModel = this.GetResource("core.user");
            var uid = userModel.Create(this.TransactionContext, userRecord);
            dynamic records = userModel.Read(this.TransactionContext, new object[] { uid }, null);
            var user1 = records[0];
            var salt = (string)user1["salt"];
            Assert.IsNull(salt);
        }

    }
}
