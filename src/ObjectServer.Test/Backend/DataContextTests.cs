using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;

using ObjectServer.Backend;
using ObjectServer.Utility;

namespace ObjectServer.Backend.Test
{
    [TestFixture]
    public class DataContextTests : LocalTestCase
    {
        [Test]
        public void Query_as_dictionary()
        {
            using (var db = DataProvider.CreateDataContext("objectserver"))
            {
                db.Open();

                var rows = db.QueryAsDictionary("SELECT id, name FROM core_model WHERE name = @0", "core.model");
                Assert.NotNull(rows);
                Assert.AreEqual(1, rows.Length);
                Assert.AreEqual("core.model", rows[0]["name"]);
            }
        }

        [Test]
        public void Query_as_datatable()
        {
            using (var db = DataProvider.CreateDataContext("objectserver"))
            {
                db.Open();

                var dt = db.QueryAsDataTable("SELECT id, name FROM core_model WHERE name = @0", "core.model");
                Assert.NotNull(dt);

                Assert.AreEqual(2, dt.Columns.Count);
                Assert.AreEqual("id", dt.Columns[0].ColumnName);
                Assert.AreEqual("name", dt.Columns[1].ColumnName);

                Assert.AreEqual(1, dt.Rows.Count);
                Assert.AreEqual("core.model", dt.Rows[0]["name"]);
            }
        }

        [Test]
        public void Query_as_dynamic()
        {
            using (var db = DataProvider.CreateDataContext("objectserver"))
            {
                db.Open();

                var records = db.QueryAsDynamic("SELECT id, name FROM core_model WHERE name = @0", "core.model");
                Assert.NotNull(records);

                Assert.AreEqual(1, records.Length);
                Assert.AreEqual("core.model", records[0].name);
            }
        }

        [Ignore]
        public void Create_and_delete_database()
        {
            var dbName = "oo_testdb";
            Platform.Initialize();
            var hash = Platform.Configuration.RootPassword.ToSha();

            var service = ServiceDispatcher.CreateDispatcher();
            service.CreateDatabase(hash, dbName, "admin");
            service.DeleteDatabase(hash, dbName);

        }
    }
}
