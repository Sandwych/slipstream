using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Xunit;

using ObjectServer.Backend;

namespace ObjectServer.Test.Backend
{
    public class DatabaseTest
    {

        [Fact]
        void Query_as_dictionary()
        {
            using (var db = DataProvider.OpenDatabase("objectserver"))
            {
                db.Open();

                var dict = db.QueryAsDictionary("select id, name from core_model");
                Assert.NotNull(dict);
                Assert.True(dict.Count > 0);
            }
        }

        [Fact]
        void Create_and_delete_database()
        {
            var dbName = "oo_testdb";
            ObjectServerStarter.Initialize();
            var sha1 = ObjectServerStarter.Configuration.RootPasswordHash;

            var service = new LocalService();
            service.CreateDatabase(sha1, dbName, "admin");
            service.DeleteDatabase(sha1, dbName);

        }
    }
}
