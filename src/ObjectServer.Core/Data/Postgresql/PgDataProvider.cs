using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NHibernate.Dialect;
using NHibernate.Driver;

namespace ObjectServer.Data.Postgresql
{
    internal class PgDataProvider : IDataProvider
    {
        private static readonly Dialect s_dialect = new PostgreSQL82Dialect();
        private static readonly IDriver s_driver = new NpgsqlDriver();

        #region IDataProvider 成员

        public IDBConnection CreateDataContext()
        {
            return new PgDBConnection();
        }

        public IDBConnection CreateDataContext(string dbName)
        {
            if (string.IsNullOrEmpty(dbName))
            {
                throw new ArgumentNullException("dbName");
            }

            return new PgDBConnection(dbName);
        }

        public string[] ListDatabases()
        {
            using (var ctx = new PgDBConnection())
            {
                ctx.Open();


                var dbUser = Platform.Configuration.DBUser;
                var sql = @"
SELECT datname FROM pg_database  
    WHERE datdba = (SELECT DISTINCT usesysid FROM pg_user WHERE usename=@0) 
        AND datname NOT IN ('template0', 'template1', 'postgres')  
    ORDER BY datname ASC
";

                var result = ctx.QueryAsDictionary(sql, dbUser);

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

            var sql = string.Format(
                @"CREATE DATABASE ""{0}"" TEMPLATE template0 ENCODING 'unicode'",
                dbName);

            using (var ctx = new PgDBConnection())
            {
                ctx.Open();

                var cmd = ctx.DBConnection.CreateCommand();
                cmd.CommandText = sql;
                cmd.ExecuteNonQuery();

                ctx.Close();
            }

        }



        public void DeleteDatabase(string dbName)
        {
            if (string.IsNullOrEmpty(dbName))
            {
                throw new ArgumentNullException("dbName");
            }

            using (var ctx = new PgDBConnection())
            {

                var sql = string.Format(
                    "DROP DATABASE \"{0}\"", dbName);

                Logger.Debug(() => "SQL: " + sql);

                var cmd = ctx.DBConnection.CreateCommand();
                cmd.CommandText = sql;
                cmd.ExecuteNonQuery();

                ctx.Close();
            }
        }

        public Dialect Dialect
        {
            get { return s_dialect; }
        }

        public IDriver Driver
        {
            get { return s_driver; }
        }

        #endregion
    }
}
