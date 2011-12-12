using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NHibernate.SqlCommand;

using NHibernate.Dialect;
using NHibernate.Driver;

namespace ObjectServer.Data.Mssql
{
    internal class MssqlDataProvider : IDataProvider
    {
        private readonly Dialect _dialect = new MsSql2005Dialect();
        private readonly DriverBase _driver = new SqlClientDriver();

        private static readonly SqlString ListDatabasesSql = SqlString.Parse(@"
select db.name from sys.sysdatabases db
inner join sys.syslogins l on l.sid = db.sid
where l.name = ?
order by l.name asc
");

        #region IDataProvider 成员

        public IDataContext OpenDataContext()
        {
            return new MssqlDataContext();
        }

        public IDataContext OpenDataContext(string dbName)
        {
            if (string.IsNullOrEmpty(dbName))
            {
                throw new ArgumentNullException("_dbName");
            }

            return new MssqlDataContext(dbName);
        }

        public string[] ListDatabases()
        {
            var dbUser = SlipstreamEnvironment.Configuration.DbUser;

            using (var ctx = this.OpenDataContext())
            {
                var result = ctx.QueryAsDictionary(ListDatabasesSql, dbUser);
                ctx.Close();

                return result.Select(e => (string)e["name"]).ToArray();
            }
        }

        public void CreateDatabase(string dbName)
        {
            if (string.IsNullOrEmpty(dbName))
            {
                throw new ArgumentNullException("_dbName");
            }

            var sql = new SqlString(
                "create database ", '"' + dbName + '"');

            using (var conn = new MssqlDataContext())
            {
                conn.Execute(sql);
                conn.Close();
            }
        }

        public void DeleteDatabase(string dbName)
        {
            if (string.IsNullOrEmpty(dbName))
            {
                throw new ArgumentNullException("_dbName");
            }

            var sql = new SqlString("drop database ", '"' + dbName + '"');

            System.Data.SqlClient.SqlConnection.ClearAllPools();
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

        public bool IsSupportProcedure { get { return false; } }

        #endregion
    }
}
