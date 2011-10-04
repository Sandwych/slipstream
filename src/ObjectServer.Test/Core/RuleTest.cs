using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;

using ObjectServer.Sql;
using ObjectServer.Model;

namespace ObjectServer.Core.Test
{

    [TestFixture]
    public sealed class RuleTest : LocalTestCase
    {
        [Test]
        public void Test_search_with_rules()
        {
            var expectedOrderNames1 = new string[] { "S/0001", "S/0004" };
            AssertSearchingOfSalesOrder("user1", "user1", expectedOrderNames1);

            var expectedOrderNames2 = new string[] { "S/0002", "S/0003", "S/0004" };
            AssertSearchingOfSalesOrder("user2", "user2", expectedOrderNames2);
        }


        private void AssertSearchingOfSalesOrder(string login, string password, string[] expectedOrderNames)
        {
            var sid = this.Service.LogOn(TestingDatabaseName, login, password);
            var ruleModel = (RuleModel)this.TransactionContext.GetResource("core.rule");
            var salesOrderModel = (IModel)this.TransactionContext.GetResource("test.sales_order");

            try
            {
                using (var tc = new TransactionContext(TestingDatabaseName, sid))
                {
                    var orders = new OrderExpression[] {
                        new OrderExpression("name", SortDirection.Ascend) 
                    };
                    var ids = salesOrderModel.SearchInternal(tc, null, orders);
                    Assert.AreEqual(expectedOrderNames.Length, ids.Length);
                    var records = salesOrderModel.ReadInternal(tc, ids);
                    var names = records.Select(r => (string)r["name"]).ToArray();
                    Assert.AreEqual(expectedOrderNames.Length, names.Length);
                    for (int i = 0; i < expectedOrderNames.Length; i++)
                    {
                        Assert.AreEqual(expectedOrderNames[i], names[i]);
                    }
                }
            }
            finally
            {
                this.Service.LogOff("objectserver", sid);
            }
        }

    }
}
