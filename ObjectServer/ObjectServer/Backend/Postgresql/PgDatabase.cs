using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;

using Npgsql;

namespace ObjectServer.Backend.Postgresql
{
    internal sealed class PgDatabase : DatabaseBase, IDatabase
    {
        private const string INITDB = "ObjectServer.Backend.Postgresql.initdb.sql";

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



        }

        public override void Initialize()
        {
            //执行初始化数据库脚本
            var assembly = Assembly.GetExecutingAssembly();
            using (var resStream = assembly.GetManifestResourceStream(INITDB))
            using (var sr = new StreamReader(resStream, Encoding.UTF8))
            {
                var text = sr.ReadToEnd();
                var lines = text.Split(';');
                foreach (var l in lines)
                {
                    if (!string.IsNullOrEmpty(l) && l.Trim().Length > 0)
                    {
                        this.Execute(l);
                    }
                }
            }

            //创建数据库只执行到这里，安装核心模块是外部的事情
        }

        #endregion

        public override ITable CreateTableHandler(IDatabase db, string tableName)
        {
            return new PgTable(db, tableName);
        }

        public override long NextSerial(string sequenceName)
        {
            var seqSql = "SELECT nextval(@0)";
            var serial = (long)this.QueryValue(seqSql, sequenceName);
            return serial;
        }

    }
}
