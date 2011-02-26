using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.Backend
{
    public static class DataProvider
    {
        private static readonly Dictionary<DatabaseType, IDataProvider> dataProviders
            = new Dictionary<DatabaseType, IDataProvider>()
        {
            { DatabaseType.Postgresql, new Postgresql.PgDataProvider() },
        };


        public static IDataContext CreateDataContext(string dbName)
        {
            var dataProvider = dataProviders[ObjectServerStarter.Configuration.DbType];
            return dataProvider.CreateDataContext(dbName);
        }

        public static IDataContext CreateDataContext()
        {
            var dataProvider = dataProviders[ObjectServerStarter.Configuration.DbType];
            return dataProvider.CreateDataContext();
        }

        public static string[] ListDatabases()
        {
            var dataProvider = dataProviders[ObjectServerStarter.Configuration.DbType];
            return dataProvider.ListDatabases();
        }

        public static void CreateDatabase(string dbName)
        {
            var dataProvider = dataProviders[ObjectServerStarter.Configuration.DbType];
            dataProvider.CreateDatabase(dbName);
        }

        public static void DeleteDatabase(string dbName)
        {
            var dataProvider = dataProviders[ObjectServerStarter.Configuration.DbType];
            dataProvider.DeleteDatabase(dbName);
        }

    }
}
