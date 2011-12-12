﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.Data
{
    //重构此类
    public static class DataProvider
    {
        private static object dataProviderLock = new object();
        private static readonly IDataProvider concreteDataProvider;

        private static readonly Dictionary<string, Type>
            dbTypeMapping = new Dictionary<string, Type>()
        {
            { "postgres",   typeof(Postgresql.PgDataProvider) },
            { "mssql",      typeof(Mssql.MssqlDataProvider) },
        };

        static DataProvider()
        {
            LoggerProvider.EnvironmentLogger.Info("Initializing DataProvider...");

            var dbTypeName = Environment.Configuration.DbType;

            if (!dbTypeMapping.ContainsKey(dbTypeName))
            {
                var msg = String.Format("Unsupported database type: [{0}]", dbTypeName);
                throw new NotSupportedException(msg);
            }

            var providerType = dbTypeMapping[dbTypeName];

            LoggerProvider.EnvironmentLogger.Info(
                String.Format("Concrete DataProvider: [{0}]", providerType.FullName));

            lock (dataProviderLock)
            {
                concreteDataProvider = Activator.CreateInstance(providerType) as IDataProvider;
            }
        }

        public static IDataContext OpenDataContext(string dbName)
        {
            if (dbName == null)
            {
                throw new ArgumentNullException("dbName");
            }

            return concreteDataProvider.OpenDataContext(dbName);
        }

        public static IDataContext CreateDataContext()
        {
            return concreteDataProvider.OpenDataContext();
        }

        public static string[] ListDatabases()
        {
            return concreteDataProvider.ListDatabases();
        }

        public static void CreateDatabase(string dbName)
        {
            if (dbName == null)
            {
                throw new ArgumentNullException("dbName");
            }

            var msg = String.Format("Creating Database: [{0}]...", dbName);
            LoggerProvider.EnvironmentLogger.Info(msg);

            concreteDataProvider.CreateDatabase(dbName);
            using (var ctx = concreteDataProvider.OpenDataContext(dbName))
            {
                LoggerProvider.EnvironmentLogger.Info("Initializing Database...");
                ctx.Initialize();
            }
        }

        public static void DeleteDatabase(string dbName)
        {
            if (dbName == null)
            {
                throw new ArgumentNullException("dbName");
            }

            var msg = String.Format("Deleting Database: [{0}]...", dbName);
            LoggerProvider.EnvironmentLogger.Info(msg);

            concreteDataProvider.DeleteDatabase(dbName);
        }

        public static NHibernate.Dialect.Dialect Dialect
        {
            get
            {
                return concreteDataProvider.Dialect;
            }
        }

        public static NHibernate.Driver.DriverBase Driver
        {
            get
            {
                return concreteDataProvider.Driver;
            }
        }

        public static bool IsSupportProcedure
        {
            get
            {
                return concreteDataProvider.IsSupportProcedure;
            }
        }

    }
}
