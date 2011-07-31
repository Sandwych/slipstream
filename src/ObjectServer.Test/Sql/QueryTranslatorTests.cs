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
    public sealed class QueryTranslatorTests
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
                " where  (_t0.login=?) and (_t1.name=?) and (_t1.code=?)");
            using (var scope = new ServiceScope("objectserver", "system"))
            {
                var cb = new QueryTranslator(scope, "core.user");

                foreach (var c in constraints)
                {
                    cb.Add(c);
                }

                var sqlStr = cb.ToSqlString();
                Assert.AreEqual(sql1.ToString(), sqlStr.ToString());

                Assert.AreEqual(cb.Values.Count, 3);
                Assert.AreEqual(cb.Values[0], "root");
                Assert.AreEqual(cb.Values[1], "org1");
                Assert.AreEqual(cb.Values[2], "orgcode1");
            }

        }

        [Test]
        public void Test_multiexp_constraints()
        {
        }


    }
}
