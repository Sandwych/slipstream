using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NHibernate.SqlCommand;

using NHibernate.Dialect;
using NHibernate.Driver;

namespace ObjectServer.Data.Postgresql
{
    internal class PgDataProvider : IDataProvider
    {
        private readonly Dialect _dialect = new PostgreSQL82Dialect();
        private readonly DriverBase _driver = new NpgsqlDriver();

        private static readonly SqlString ListDatabasesSql = SqlString.Parse(@"
select datname from pg_database  
    where datdba = (select distinct usesysid from pg_user where usename=?) 
        and datname not in ('template0', 'template1', 'postgres')  
    order by datname asc
");

        #region IDataProvider 成员

        public IDataContext OpenDataContext()
        {
            return new PgDataContext();
        }

        public IDataContext OpenDataContext(string dbName)
        {
            if (string.IsNullOrEmpty(dbName))
            {
                throw new ArgumentNullException("_dbName");
            }

            return new PgDataContext(dbName);
        }

        public string[] ListDatabases()
        {
            var dbUser = SlipstreamEnvironment.Settings.DbUser;

            using (var ctx = this.OpenDataContext())
            {
                var result = ctx.QueryAsDictionary(ListDatabasesSql, dbUser);
                ctx.Close();

                return result.Select(e => (string)e["datname"]).ToArray();
            }
        }

        public void CreateDatabase(string dbName)
        {
            if (string.IsNullOrEmpty(dbName))
            {
                throw new ArgumentNullException("_dbName");
            }

            var msg = String.Format("Creating Database: [{0}]...", dbName);
            LoggerProvider.EnvironmentLogger.Info(msg);

            using (var conn = new PgDataContext())
            {
                var sql = new SqlString(
                    " create database ", '"' + dbName + '"',
                    " template template0 encoding 'unicode'");
                conn.Execute(sql);
            }

            using (var conn = new PgDataContext(dbName))
            {
                LoggerProvider.EnvironmentLogger.Info("Initializing Database...");
                conn.Setup();
            }


        }

        public void DeleteDatabase(string dbName)
        {
            if (string.IsNullOrEmpty(dbName))
            {
                throw new ArgumentNullException("_dbName");
            }

            var sql = new SqlString("drop database " + '"' + dbName + '"');

            Npgsql.NpgsqlConnection.ClearAllPools();
            using (var conn = this.OpenDataContext())
            {
                conn.Execute(sql);
                conn.Close();
            }
        }

        public Dialect Dialect
        {
            get { return _dialect; }
        }

        public IDriver Driver
        {
            get { return _driver; }
        }

        public bool IsSupportProcedure { get { return true; } }

        #endregion
    }
}
