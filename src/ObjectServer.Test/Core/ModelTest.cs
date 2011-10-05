using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;

namespace ObjectServer.Core.Test
{

    [TestFixture]
    public sealed class ModelTest : UserLoggedTestCaseBase
    {
        [Test]
        public void Test_GetFields()
        {
            var modelName = "core.user";
            dynamic userModel = this.GetResource("core.model");
            var result = userModel.GetFields(this.TransactionContext, modelName);
            var records = ((object[])result).Select(i => (Dictionary<string, object>)i);

            Assert.IsTrue(records.Count() > 0);
        }

    }
}
