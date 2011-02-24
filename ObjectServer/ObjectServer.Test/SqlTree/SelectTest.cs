using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;

namespace ObjectServer.SqlTree.Test
{
    [TestFixture]
    public class SelectTest
    {

        [Test]
        public void Test_select_statement()
        {
            var sql = @"
SELECT ""col1"", ""col2"", ""col3"" 
    FROM ""table1"" AS ""t1""
    LEFT JOIN ""table2"" AS ""t2"" ON ""t2"".""table1_id"" = ""t1"".""id""
    WHERE (""col1"" = 123) AND (""col2"" LIKE 'exp3') 
    ORDER BY ""id"" ASC
"
            .Replace(" ", "").Replace("\n", "").Replace("\r", "");

            var cols = new AliasExpressionList(new string[] { 
                    "col1", "col2", "col3", });

            var whereExp = new BinaryExpression(
                new BracketedExpression(
                new BinaryExpression("col1", "=", 123)),
                ExpressionOperator.AndOperator,
                new BracketedExpression(
                new BinaryExpression(
                    new IdentifierExpression("col2"),
                    ExpressionOperator.LikeOperator,
                    new ValueExpression("exp3"))));

            IExpression joinExp = new BinaryExpression(
                new BinaryExpression(
                    new IdentifierExpression("t2"),
                    new ExpressionOperator("."),
                    new IdentifierExpression("table1_id")),
                    ExpressionOperator.EqualOperator,
                new BinaryExpression(
                    new IdentifierExpression("t1"),
                    new ExpressionOperator("."),
                    new IdentifierExpression("id")));

            var joinClause = new JoinClause(
                "LEFT",
                new AliasExpression("table2", "t2"),
                joinExp);


            var fromClause = new FromClause(new AliasExpression("table1", "t1"));
            var select = new SelectStatement(cols, fromClause);
            select.JoinClause = joinClause;
            select.WhereClause = new WhereClause(whereExp);
            select.OrderByClause = new OrderbyClause("id", "asc");

            var genSql = GenerateSqlString(select);

            Assert.AreEqual(sql, genSql);
        }


        [Test]
        public void Test_raw_sql_node()
        {
            var sql = "SELECT \"col\" FROM \"table1\" WHERE col = true";
            var sql1 = sql.Replace(" ", "");
            var select = new RawSql(sql);
            var genSql = GenerateSqlString(select);
            Assert.AreEqual(sql1, genSql);
        }

        [Test]
        public void Test_simplest_expression_node()
        {
            var sql = "\"col\"=1 AND \"name\" LIKE '%myname%'";
            var sql1 = sql.Replace(" ", "");

            var exp1 = new BinaryExpression(
                new IdentifierExpression("col"),
                ExpressionOperator.EqualOperator,
                new ValueExpression(1));

            var exp2 = new BinaryExpression(
                new IdentifierExpression("name"),
                ExpressionOperator.LikeOperator,
                new ValueExpression("%myname%"));

            var exp3 = new BinaryExpression(
                exp1, ExpressionOperator.AndOperator, exp2);

            var genSql = GenerateSqlString(exp3);
            Assert.AreEqual(sql1, genSql);
        }


        private static string GenerateSqlString(INode node)
        {
            var genSql = node.ToString().Replace(" ", "");
            genSql = genSql.Replace("\t", "");
            return genSql;
        }
    }
}
