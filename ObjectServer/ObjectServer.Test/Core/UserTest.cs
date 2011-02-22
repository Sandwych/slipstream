﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;

namespace ObjectServer.Core.Test
{

    [TestFixture]
    public sealed class UserTest : TestBase
    {
        IService proxy = new LocalService();

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

            var uid = proxy.CreateModel("objectserver", UserModel.ModelName, userRecord);
            var records = proxy.ReadModel("objectserver", UserModel.ModelName, new object[] { uid }, null);
            var user1 = records[0];

            var salt = (string)user1["salt"];
            Assert.IsNull(salt);
        }

    }
}
