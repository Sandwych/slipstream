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
select ""col1"", ""col2"", ""col3"" 
    from ""table1"" as ""t1""
    left join ""table2"" as ""t2"" on ""t2"".""table1_id"" = ""t1"".""id""
    where (""col1"" = 123) and (""col2"" like 'exp3') 
"
            .Replace(" ", "").Replace("\n", "").Replace("\r", "");

            var cols = new AliasExpressionList(new string[] { 
                    "col1", "col2", "col3", });

            var whereExp = new BinaryExpression(
                new BracketedExpression(
                new BinaryExpression("col1", "=", 123)),
                new ExpressionOperator("and"),
                new BracketedExpression(
                new BinaryExpression(
                    new IdentifierExpression("col2"),
                    new ExpressionOperator("like"),
                    new ValueExpression("exp3"))));

            IExpression joinExp = new BinaryExpression(
                new BinaryExpression(
                    new IdentifierExpression("t2"),
                    new ExpressionOperator("."),
                    new IdentifierExpression("table1_id")),
                new ExpressionOperator("="),
                new BinaryExpression(
                    new IdentifierExpression("t1"),
                    new ExpressionOperator("."),
                    new IdentifierExpression("id")));

            var joinClause = new JoinClause(
                "left",
                new AliasExpression("table2", "t2"),
                joinExp);


            var fromClause = new FromClause(new AliasExpression("table1", "t1"));
            var select = new SelectStatement(cols, fromClause);
            select.JoinClause = joinClause;
            select.WhereClause = new WhereClause(whereExp);

            var genSql = GenerateSqlString(select);

            Assert.AreEqual(sql, genSql);
        }

        private static string GenerateSqlString(INode node)
        {
            var genSql = node.ToString().Replace(" ", "");
            genSql = genSql.Replace("\t", "");
            return genSql;
        }

        [Test]
        public void Test_raw_sql_node()
        {
            var sql = "select \"col\" from \"table1\" where col = true";
            var sql1 = sql.Replace(" ", "");
            var select = new RawSql(sql);
            var genSql = GenerateSqlString(select);
            Assert.AreEqual(sql1, genSql);
        }

        [Test]
        public void Test_simplest_expression_node()
        {
            var sql = "\"col\"=1 and \"name\" like '%myname%'";
            var sql1 = sql.Replace(" ", "");

            var exp1 = new BinaryExpression(
                new IdentifierExpression("col"),
                new ExpressionOperator("="),
                new ValueExpression(1));

            var exp2 = new BinaryExpression(
                new IdentifierExpression("name"),
                new ExpressionOperator("like"),
                new ValueExpression("%myname%"));

            var exp3 = new BinaryExpression(
                exp1, new ExpressionOperator("and"), exp2);

            var genSql = GenerateSqlString(exp3);
            Assert.AreEqual(sql1, genSql);
        }
    }
}
