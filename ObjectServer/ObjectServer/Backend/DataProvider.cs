using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.Backend
{
    public sealed class DataProvider
    {
        private static readonly DataProvider s_instance = new DataProvider();

        private readonly Dictionary<DatabaseType, Type> dbTypes =
            new Dictionary<DatabaseType, Type>()
            {
                { DatabaseType.Postgresql, typeof(Postgresql.PgDatabase) },
            };

        private DataProvider()
        {
        }

        public static IDatabase OpenDatabase(string dbName)
        {
            var dbType = s_instance.dbTypes[ObjectServerStarter.Configuration.DbType];
            //var db = Activator.CreateInstance(dbType);
            var db = new DatabaseBase(dbName);
            db.Open();
            return db;
        }

    }
}
