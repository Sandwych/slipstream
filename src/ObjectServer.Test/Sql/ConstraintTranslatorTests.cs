using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using NHibernate.SqlCommand;

using ObjectServer.Model;
using ObjectServer.Sql;

namespace ObjectServer.Sql.Test
{
    [TestFixture]
    public sealed class ConstraintTranslatorTests
    {
        [TestFixtureSetUp]
        public void TestFixtureSetup()
        {
            if (!Platform.Initialized)
            {
                Platform.Initialize();
            }
        }

        [Test]
        public void Test_simple_constraints()
        {
            var constraints = new ConstraintExpression[] {
                new ConstraintExpression("login", "=", "root"),
                new ConstraintExpression("organization.name", "=", "org1"),
                new ConstraintExpression("organization.code", "=", "orgcode1"),
            };

            var sql1 = new SqlString(
                "select distinct _t0._id from core_user _t0",
                " left outer join core_organization _t1 on _t0.organization=_t1._id",
                " where  (_t0.login=?) and (_t1.name=?) and (_t1.code=?)",
                " order by  _t0.login ASC,  _t0.name ASC");
            using (var scope = new ServiceScope("objectserver", "system"))
            {
                var cb = new ConstraintTranslator(scope, "core.user");

                foreach (var c in constraints)
                {
                    cb.Add(c);
                }
                cb.SetOrder(new OrderExpression("login", SortDirection.Asc));
                cb.SetOrder(new OrderExpression("name", SortDirection.Asc));

                var sqlStr = cb.ToSqlString();
                Assert.AreEqual(
                    sql1.ToString().Replace(" ", ""),
                    sqlStr.ToString().Replace(" ", ""));

                Assert.AreEqual(cb.Values.Length, 3);
                Assert.AreEqual(cb.Values[0], "root");
                Assert.AreEqual(cb.Values[1], "org1");
                Assert.AreEqual(cb.Values[2], "orgcode1");
            }

        }
    }
}
