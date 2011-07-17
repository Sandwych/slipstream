using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;

using ObjectServer.Model;

namespace ObjectServer.Core.Test
{

    [TestFixture]
    public sealed class RuleTest : LocalTestCase
    {
        [Test]
        public void Test_GetRuleDomain()
        {
            var sid = this.Service.LogOn("objectserver", "user1", "user1");
            var ruleModel = (RuleModel)this.ServiceScope.GetResource("core.rule");
            var salesOrderModel = (IModel)this.ServiceScope.GetResource("test.sales_order");
            var userModel = (UserModel)this.ServiceScope.GetResource("core.user");

            try
            {
                using (var scope = new ServiceScope(sid))
                {
                    dynamic user = userModel.Browse(scope, scope.Session.UserId);

                    var domains = RuleModel.GetRuleDomain(scope, "test.sales_order", "read");
                    Assert.AreEqual(1, domains.Length); //只有一组rule
                    Assert.AreEqual("organization.id", domains[0].Field);
                    Assert.AreEqual("=", domains[0].Operator);
                    Assert.AreEqual(user.organization.id, domains[0].Value);
                }
            }
            finally
            {
                this.Service.LogOff(sid);
            }


        }

    }
}
