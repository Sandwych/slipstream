using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;

using ObjectServer.Model;

namespace ObjectServer.Model.Test
{
    [TestFixture]
    public sealed class DomainParserTests : LocalTestCase
    {

        [Test]
        public void Test_simple_constraints()
        {
            var model = (IModel)this.ServiceScope.GetResource("core.user");

            var constraints = new ConstraintExpression[] {
                new ConstraintExpression("login", "=", "root"),
            };

            var dp = new ConstraintBuilder(this.ServiceScope, model);
            var result = dp.Push(constraints);

            var aliases = dp.GetAllAliases();
            Assert.AreEqual(1, dp.GetAllAliases().Length);
            Assert.AreEqual("\"core_user\"", aliases[0].Lhs.ToString());
            Assert.AreEqual("\"_t0\"", aliases[0].Rhs.ToString());
        }

        [Test]
        public void Test_multiexp_constraints()
        {
            var model = (IModel)this.ServiceScope.GetResource("core.user");

            var constraints = new ConstraintExpression[] {
                new ConstraintExpression("login", "=", "root"),
                new ConstraintExpression("organization.code", "=", "org1"),
                new ConstraintExpression("organization.name", "=", "my org"),
            };

            var dp = new ConstraintBuilder(this.ServiceScope, model);
            var result = dp.Push(constraints);

            var aliases = dp.GetAllAliases();
            Assert.AreEqual(2, aliases.Length);
            Assert.AreEqual("\"core_user\"", aliases[0].Lhs.ToString());
            Assert.AreEqual("\"_t0\"", aliases[0].Rhs.ToString());
            Assert.AreEqual("\"core_organization\"", aliases[1].Lhs.ToString());
            Assert.AreEqual("\"_t1\"", aliases[1].Rhs.ToString());
        }


    }
}
