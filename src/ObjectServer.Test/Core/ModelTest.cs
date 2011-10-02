using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;

namespace ObjectServer.Core.Test
{

    [TestFixture]
    public sealed class ModelTest : LocalTestCase
    {
        [Test]
        public void Test_GetFields()
        {
            var modelName = "core.user";

            var result = this.Service.Execute(TestingDatabaseName, this.SessionId, "core.model", "GetFields", modelName);
            var records = ((object[])result).Select(i => (Dictionary<string, object>)i);

            Assert.IsTrue(records.Count() > 0);
        }

    }
}
