using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.Data
{
    //重构此类
    public static class DataProvider
    {
        private static readonly Dictionary<DatabaseType, IDataProvider> dataProviders
            = new Dictionary<DatabaseType, IDataProvider>()
        {
            { DatabaseType.Postgresql, new Postgresql.PgDataProvider() },
        };

        private static readonly Dictionary<string, Data.DatabaseType>
            dbTypeMapping = new Dictionary<string, DatabaseType>()
        {
            { "postgres", Data.DatabaseType.Postgresql },
        };


        public static IDBConnection CreateDataContext(string dbName)
        {
            if (dbName == null)
            {
                throw new ArgumentNullException("dbName");
            }

            var dbType = dbTypeMapping[Platform.Configuration.DbType];
            var dataProvider = dataProviders[dbType];
            return dataProvider.CreateDataContext(dbName);
        }

        public static IDBConnection CreateDataContext()
        {
            var dbType = dbTypeMapping[Platform.Configuration.DbType];
            var dataProvider = dataProviders[dbType];
            return dataProvider.CreateDataContext();
        }

        public static string[] ListDatabases()
        {
            var dbType = dbTypeMapping[Platform.Configuration.DbType];
            var dataProvider = dataProviders[dbType];
            return dataProvider.ListDatabases();
        }

        public static void CreateDatabase(string dbName)
        {
            if (dbName == null)
            {
                throw new ArgumentNullException("dbName");
            }

            var dbType = dbTypeMapping[Platform.Configuration.DbType];
            var dataProvider = dataProviders[dbType];
            dataProvider.CreateDatabase(dbName);
        }

        public static void DeleteDatabase(string dbName)
        {
            if (dbName == null)
            {
                throw new ArgumentNullException("dbName");
            }

            var dbType = dbTypeMapping[Platform.Configuration.DbType];
            var dataProvider = dataProviders[dbType];
            dataProvider.DeleteDatabase(dbName);
        }

        public static NHibernate.Dialect.Dialect Dialect
        {
            get
            {
                var dbType = dbTypeMapping[Platform.Configuration.DbType];
                var dataProvider = dataProviders[dbType];
                return dataProvider.Dialect;
            }
        }

        public static NHibernate.Driver.IDriver Driver
        {
            get
            {
                var dbType = dbTypeMapping[Platform.Configuration.DbType];
                var dataProvider = dataProviders[dbType];
                return dataProvider.Driver;
            }
        }

    }
}
