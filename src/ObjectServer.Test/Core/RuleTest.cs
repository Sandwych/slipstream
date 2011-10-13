using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;

using ObjectServer.Model;

namespace ObjectServer.Core.Test
{

    [TestFixture]
    public sealed class RuleTest : TransactionTestCaseBase
    {
        [Test]
        public void CanSearchWithRules()
        {
            var expectedOrderNames1 = new string[] { "S/0001", "S/0004" };
            AssertSearchingOfSalesOrder("user1", "user1", expectedOrderNames1);

            var expectedOrderNames2 = new string[] { "S/0002", "S/0003", "S/0004" };
            AssertSearchingOfSalesOrder("user2", "user2", expectedOrderNames2);
        }


        private void AssertSearchingOfSalesOrder(string login, string password, string[] expectedOrderNames)
        {
            var services = Environment.ExportedService;

            var sid = services.LogOn(TestingDatabaseName, login, password);
            //var ruleModel = (RuleModel)this.TransactionContext.GetResource("core.rule");
            //var salesOrderModel = (IModel)this.TransactionContext.GetResource("test.sales_order");

            try
            {
                var orders = new OrderExpression[] {
                        new OrderExpression("name", SortDirection.Ascend) 
                    };
                var ids = (long[])services.Execute(TransactionContextTestCaseBase.TestingDatabaseName, sid, "test.sales_order", "Search",
                    null, null, 0, 0);
                Assert.AreEqual(expectedOrderNames.Length, ids.Length);
                var records = (Dictionary<string, object>[])services.Execute(
                    TransactionContextTestCaseBase.TestingDatabaseName, sid, "test.sales_order", "Read",
                    ids, null);
                var names = records.Select(r => (string)r["name"]).ToArray();
                Assert.AreEqual(expectedOrderNames.Length, names.Length);
                for (int i = 0; i < expectedOrderNames.Length; i++)
                {
                    Assert.AreEqual(expectedOrderNames[i], names[i]);
                }
            }
            finally
            {
                services.LogOff(TestingDatabaseName, sid);
            }
        }


    }
}
