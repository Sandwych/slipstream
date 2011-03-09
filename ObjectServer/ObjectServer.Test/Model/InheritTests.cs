using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;

using ObjectServer.Model;

namespace ObjectServer.Model.Test
{
    [TestFixture]
    public class InheritTests : LocalTestBase
    {
        [Test]
        public void Test_single_table()
        {
            using (var ctx = new ContextScope(new Guid(this.SessionId)))
            {
                dynamic inheritedModel = ctx.Database.GetResource("test.single_table");
                Assert.True(inheritedModel.Fields.ContainsKey("age"));
            }
        }

    }
}
