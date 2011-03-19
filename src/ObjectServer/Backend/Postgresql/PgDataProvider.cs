using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.Backend.Postgresql
{
    internal class PgDataProvider : IDataProvider
    {

        #region IDataProvider 成员

        public IDataContext CreateDataContext()
        {
            return new PgDataContext();
        }

        public IDataContext CreateDataContext(string dbName)
        {
            if (string.IsNullOrEmpty(dbName))
            {
                throw new ArgumentNullException("dbName");
            }

            return new PgDataContext(dbName);
        }

        public string[] ListDatabases()
        {
            using (var ctx = new PgDataContext())
            {
                ctx.Open();


                var dbUser = ObjectServerStarter.Configuration.DBUser;
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

            using (var ctx = new PgDataContext())
            {
                ctx.Open();

                var cmd = ctx.Connection.CreateCommand();
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

            using (var ctx = new PgDataContext())
            {

                var sql = string.Format(
                    "DROP DATABASE \"{0}\"", dbName);

                Logger.Debug(() => "SQL: " + sql);

                var cmd = ctx.Connection.CreateCommand();
                cmd.CommandText = sql;
                cmd.ExecuteNonQuery();

                ctx.Close();
            }
        }

        #endregion
    }
}
