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
        public void Test_simplest_select_statement()
        {
            var sql = "select \"col1\", \"col2\", \"col3\" from \"table1\"".Replace(" ", "");
            var select = new SelectStatement();
            select.ColumnList = new ColumnList(new string[] { "col1", "col2", "col3" });
            select.FromClause = new FromClause(new string[] { "table1" });

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
    }
}
