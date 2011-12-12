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
        private static readonly Dialect s_dialect = new PostgreSQL82Dialect();
        private static readonly DriverBase s_driver = new NpgsqlDriver();

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
                throw new ArgumentNullException("dbName");
            }

            return new PgDataContext(dbName);
        }

        public string[] ListDatabases()
        {
            var dbUser = Environment.Configuration.DbUser;

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
                throw new ArgumentNullException("dbName");
            }

            var sql = new SqlString(
                "create database ",
                DataProvider.Dialect.QuoteForSchemaName(dbName),
                " template template0 encoding 'unicode'");

            using (var conn = new PgDataContext())
            {
                conn.Execute(sql);
                conn.Close();
            }
        }

        public void DeleteDatabase(string dbName)
        {
            if (string.IsNullOrEmpty(dbName))
            {
                throw new ArgumentNullException("dbName");
            }

            var sql = new SqlString(
                  "drop database ",
                  DataProvider.Dialect.QuoteForSchemaName(dbName));

            Npgsql.NpgsqlConnection.ClearAllPools();
            using (var conn = this.OpenDataContext())
            {
                conn.Execute(sql);
                conn.Close();
            }
        }

        public Dialect Dialect
        {
            get { return s_dialect; }
        }

        public DriverBase Driver
        {
            get { return s_driver; }
        }

        public bool IsSupportProcedure { get { return true; } }

        #endregion
    }
}
