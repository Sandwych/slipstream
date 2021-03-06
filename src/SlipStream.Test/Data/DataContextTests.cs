﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NHibernate.SqlCommand;
using NUnit.Framework;
using Sandwych.Utility;
using Sandwych;

using SlipStream.Data;

namespace SlipStream.Data.Test
{
    [TestFixture(Category = "ORM")]
    public class DataContextTests : ServiceContextTestCaseBase
    {
        [Test]
        public void Query_as_dictionary()
        {
            using (var db = this.DbDomain.DataProvider.OpenDataContext(TestingDatabaseName))
            {
                var sql = "SELECT _id, name FROM core_meta_entity WHERE name=?";
                var rows = db.QueryAsDictionary(sql, Core.MetaEntityEntity.EntityName);
                Assert.NotNull(rows);
                Assert.AreEqual(1, rows.Length);
                Assert.AreEqual(Core.MetaEntityEntity.EntityName, rows[0]["name"]);
            }
        }

        [Test]
        public void Query_as_datatable()
        {
            using (var db = this.DbDomain.DataProvider.OpenDataContext(TestingDatabaseName))
            {
                var sql = "SELECT _id, name FROM core_meta_entity WHERE name=?";
                var dt = db.QueryAsDataTable(sql, Core.MetaEntityEntity.EntityName);
                Assert.NotNull(dt);

                Assert.AreEqual(2, dt.Columns.Count);
                Assert.AreEqual("_id", dt.Columns[0].ColumnName);
                Assert.AreEqual("name", dt.Columns[1].ColumnName);

                Assert.AreEqual(1, dt.Rows.Count);
                Assert.AreEqual(Core.MetaEntityEntity.EntityName, dt.Rows[0]["name"]);
            }
        }

        [Test]
        public void Query_as_dynamic()
        {
            using (var db = this.DbDomain.DataProvider.OpenDataContext(TestingDatabaseName))
            {
                var sql = "SELECT _id, name FROM core_meta_entity WHERE name=?";
                var records = db.QueryAsDynamic(sql, Core.MetaEntityEntity.EntityName);
                Assert.NotNull(records);

                Assert.AreEqual(1, records.Length);
                Assert.AreEqual(Core.MetaEntityEntity.EntityName, records[0].name);
            }
        }


        [Test]
        public void Query_as_array()
        {
            using (var db = this.DbDomain.DataProvider.OpenDataContext(TestingDatabaseName))
            {
                var sql = "SELECT name FROM core_meta_entity WHERE name=?";
                var names = db.QueryAsArray<string>(sql, Core.MetaEntityEntity.EntityName);

                Assert.NotNull(names);
                Assert.AreEqual(1, names.Length);
                Assert.AreEqual(Core.MetaEntityEntity.EntityName, names[0]);
            }
        }

        public void Create_and_delete_database()
        {
            var dbName = "oo_testdb";
            SlipstreamEnvironment.Initialize();
            var hash = SlipstreamEnvironment.Settings.ServerPassword.ToSha();

            var service = SlipstreamEnvironment.RootService;
            service.CreateDatabase(hash, dbName, "admin");
            service.DeleteDatabase(hash, dbName);

        }
    }
}
