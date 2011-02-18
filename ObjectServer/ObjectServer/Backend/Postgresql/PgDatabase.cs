using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Npgsql;

namespace ObjectServer.Backend.Postgresql
{
    public class PgDatabase : DatabaseBase, IDatabase
    {
        public PgDatabase(string dbName)
        {
            var cfg = ObjectServerStarter.Configuration;
            string connectionString = string.Format(
              "Server={0};" +
              "Database={3};" +
              "Encoding=UNICODE;" +
              "User ID={1};" +
              "Password={2};",
              cfg.DbHost, cfg.DbUser, cfg.DbPassword, dbName);
            this.conn = new NpgsqlConnection(connectionString);
            this.DatabaseName = dbName;
        }

        public PgDatabase()
            : this("template1")
        {
        }

        #region IDatabase 成员

        public override string[] List()
        {
            EnsureConnectionOpened();

            var dbUser = "objectserver"; //TODO: 改成可配置的
            var sql = @"
                select datname from pg_database  
                    where datdba = (select distinct usesysid from pg_user where usename=@0) 
                        and datname not in ('template0', 'template1', 'postgres')  
	                order by datname asc;";

            if (Log.IsDebugEnabled)
            {
                Log.Info(sql);
            }

            using (var cmd = this.PrepareCommand(sql))
            {
                PrepareCommandParameters(cmd, dbUser);
                var result = new List<string>();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(reader.GetString(0));
                    }
                }
                return result.ToArray();
            }
        }


        public override void Create(string dbName)
        {
            EnsureConnectionOpened();

            //TODO 检查连接
            var sql = string.Format(
                @"CREATE DATABASE ""{0}"" TEMPLATE template0 ENCODING 'unicode'",
                dbName);

            if (Log.IsDebugEnabled)
            {
                Log.Info(sql);
            }

            var cmd = this.Connection.CreateCommand();
            cmd.CommandText = sql;
            cmd.ExecuteNonQuery();

            this.DatabaseName = dbName;
        }

        #endregion

    }
}
