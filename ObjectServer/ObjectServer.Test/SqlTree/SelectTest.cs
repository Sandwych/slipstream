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
    from ""table1""
    where ""col1"" = 123 and ""col2"" like 'exp3' 
"
            .Replace(" ", "").Replace("\n", "").Replace("\r", "");

            var cols = new ExpressionList(
                new IExpression[] { 
                    new AliasExpression("col1"), 
                    new AliasExpression("col2"), 
                    new AliasExpression("col3"), 
                });

            var whereExp = new BinaryExpression(
                new BinaryExpression("col1", "=", 123),
                new ExpressionOperator("and"),
                new BinaryExpression("col2", "like", "exp3"));

            var fromClause = new FromClause(new string[] { "table1" });
            var select = new SelectStatement(cols, fromClause);
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
