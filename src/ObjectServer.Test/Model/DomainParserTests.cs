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
        public void Test_simple_domain()
        {
            var model = (IModel)this.ServiceScope.GetResource("core.user");

            var domain = new DomainExpression[] {
                new DomainExpression("login", "=", "root"),
            };

            var dp = new DomainParser(this.ServiceScope, model);
            var result = dp.Parse(domain);

            Assert.AreEqual(1, result.Item1.Length);
            Assert.AreEqual("\"core_user\"", result.Item1[0].Lhs.ToString());
            Assert.AreEqual("\"_t0\"", result.Item1[0].Rhs.ToString());
        }

        [Test]
        public void Test_multiexp_domain()
        {
            var model = (IModel)this.ServiceScope.GetResource("core.user");

            var domain = new DomainExpression[] {
                new DomainExpression("login", "=", "root"),
                new DomainExpression("organization.code", "=", "org1"),
                new DomainExpression("organization.name", "=", "my org"),
            };

            var dp = new DomainParser(this.ServiceScope, model);
            var result = dp.Parse(domain);

            Assert.AreEqual(2, result.Item1.Length);
            Assert.AreEqual("\"core_user\"", result.Item1[0].Lhs.ToString());
            Assert.AreEqual("\"_t0\"", result.Item1[0].Rhs.ToString());
            Assert.AreEqual("\"core_organization\"", result.Item1[1].Lhs.ToString());
            Assert.AreEqual("\"_t1\"", result.Item1[1].Rhs.ToString());
        }


    }
}
