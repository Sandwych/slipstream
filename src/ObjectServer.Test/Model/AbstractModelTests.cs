using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;

using NUnit.Framework;

using ObjectServer.Model;

namespace ObjectServer.Model.Test
{

    [TestFixture]
    public class AbstractModelTests : TransactionContextTestCaseBase
    {
        [Test]
        public void ShouldHandleWithBadConstraints()
        {
            dynamic model = this.GetResource("core.model");
            var constraints = new object[] {
                new object[] { "kk", "=", 13 },
            };

            Assert.Throws<ArgumentException>(delegate
            {
                var ids = model.Search(this.TransactionContext, constraints, null, 0, 0);
            });

            Assert.Throws<ArgumentException>(delegate
            {
                var ids = model.Count(this.TransactionContext, constraints);
            });

        }
    }
}
