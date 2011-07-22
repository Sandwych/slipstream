using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;

using ObjectServer.SqlTree;
namespace ObjectServer.SqlTree.Test
{

    [TestFixture]
    public class ExpressionExtensionsTests
    {

        [Test]
        public void Test_JoinExpressions_odd()
        {
            var exps = new IExpression[] {
                new ValueExpression(1),
                new ValueExpression(2),
                new ValueExpression(3),
                new ValueExpression(4),
                new ValueExpression(5),
            };

            var joinedExp = exps.JoinExpressions(ExpressionOperator.AndOperator);
            var expStr = joinedExp.ToString().Replace(" ", "");
            Assert.AreEqual("1AND2AND3AND4AND5", expStr);

        }

        public void Test_JoinExpressions_even()
        {
            var exps = new IExpression[] {
                new ValueExpression(1),
                new ValueExpression(2),
                new ValueExpression(3),
                new ValueExpression(4),
            };

            var joinedExp = exps.JoinExpressions(ExpressionOperator.AndOperator);
            var expStr = joinedExp.ToString().Replace(" ", "");
            Assert.AreEqual("1AND2AND3AND4", expStr);
        }

        public void Test_JoinExpressions_single()
        {
            var exps = new IExpression[] {
                new ValueExpression(1),
            };

            var joinedExp = exps.JoinExpressions(ExpressionOperator.AndOperator);
            var expStr = joinedExp.ToString().Replace(" ", "");
            Assert.AreEqual("1", expStr);
        }

        public void Test_JoinExpressions_two()
        {
            var exps = new IExpression[] {
                new ValueExpression(1),
                new ValueExpression(2),
            };

            var joinedExp = exps.JoinExpressions(ExpressionOperator.AndOperator);
            var expStr = joinedExp.ToString().Replace(" ", "");
            Assert.AreEqual("1AND2", expStr);
        }
    }
}
