using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;

using ObjectServer.SqlTree;
namespace ObjectServer.SqlTree.Test
{

    [TestFixture]
    public class UpdateTests
    {

        [Test]
        public void TestSimpleUpdateStatement()
        {
            var sql = "UPDATE \"table1\" SET \"name\"='GOOD', \"age\"=23";
            var sql1 = sql.Replace(" ", "");

            var setExps = new BinaryExpression[] {
                        new BinaryExpression(
                            new IdentifierExpression("name"), 
                            ExpressionOperator.EqualOperator, 
                            new ValueExpression("GOOD")), 
                        new BinaryExpression(
                            new IdentifierExpression("age"),
                            ExpressionOperator.EqualOperator,
                            new ValueExpression(23)),
            };

            var updateStatement = new UpdateStatement(
                new AliasExpression("table1"),
                new SetClause(setExps));


            Assert.AreEqual(sql1, updateStatement.ToString().Replace(" ", ""));

        }
    }
}
