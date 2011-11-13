using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using System.Globalization;

using NHibernate.SqlCommand;

namespace ObjectServer.Data.Postgresql
{
    internal sealed class PgDBContext : AbstractDbContext, IDbContext
    {
        private readonly static Type pgt = typeof(Npgsql.NpgsqlCommand);

        private const string INITDB = "ObjectServer.Data.Postgresql.initdb.sql";
        private readonly static SqlString SqlToListDBs = SqlString.Parse(@"
                select datname from pg_database  
                    where datdba = (select distinct usesysid from pg_user where usename=?) 
                        and datname not in ('template0', 'template1', 'postgres')  
	                order by datname asc;");


        public PgDBContext(string dbName)
        {
            if (string.IsNullOrEmpty(dbName))
            {
                throw new ArgumentNullException("dbName");
            }

            var cfg = Environment.Configuration;
            string connectionString = string.Format(
              CultureInfo.InvariantCulture,
              "Server={0};" +
              "Database={3};" +
              "Encoding=UNICODE;" +
              "User ID={1};" +
              "Password={2};",
              cfg.DbHost, cfg.DbUser, cfg.DbPassword, dbName);
            var dbc = DataProvider.Driver.CreateConnection();
            dbc.ConnectionString = connectionString;
            //this.conn = new NpgsqlConnection(connectionString);
            this.conn = dbc;
            this.DatabaseName = dbName;
        }

        public PgDBContext()
            : this("template1")
        {
        }

        #region IDatabase 成员

        public override string[] List()
        {
            EnsureConnectionOpened();

            var dbUser = Environment.Configuration.DbUser;

            return this.QueryAsArray<string>(SqlToListDBs, dbUser);
        }


        public override void Create(string dbName)
        {
            if (string.IsNullOrEmpty(dbName))
            {
                throw new ArgumentNullException("dbName");
            }

            EnsureConnectionOpened();

            LoggerProvider.EnvironmentLogger.Info(String.Format("Creating Database [{0}]...", dbName));

            var sqlBuilder = new SqlStringBuilder();
            sqlBuilder.Add("create database ");
            sqlBuilder.Add(DataProvider.Dialect.QuoteForSchemaName(dbName));
            sqlBuilder.Add(" template template0 encoding 'unicode' ");

            var sql = sqlBuilder.ToSqlString();

            this.Execute(sql);

            LoggerProvider.EnvironmentLogger.Info(String.Format("Database [{0}] has been created.", dbName));
        }

        public override void Initialize()
        {
            LoggerProvider.EnvironmentLogger.Info(
                String.Format("Executing the database initialization script [{0}]...", INITDB));

            //执行初始化数据库脚本
            var assembly = Assembly.GetExecutingAssembly();
            using (var resStream = assembly.GetManifestResourceStream(INITDB))
            using (var sr = new StreamReader(resStream, Encoding.UTF8))
            {
                string line = null;
                var sb = new SqlStringBuilder();
                while ((line = sr.ReadLine()) != null)
                {
                    if (string.IsNullOrEmpty(line) || line.Trim().Length == 0)
                    {
                        continue;
                    }

                    if (line.Trim().ToUpperInvariant() == "GO")
                    {
                        this.Execute(sb.ToSqlString());
                        sb.Clear();
                    }
                    else
                    {
                        sb.Add(line + '\n');
                    }
                }
            }

            //创建数据库只执行到这里，安装核心模块是外部的事情
            LoggerProvider.EnvironmentLogger.Info("The database scheme has been initialized.");
        }

        public override bool IsInitialized()
        {
            var sql = SqlString.Parse(@"
select distinct count(table_name) 
    from information_schema.tables 
    where table_name in ('core_module', 'core_model', 'core_field')
");
            var rowCount = (long)this.QueryValue(sql);
            return rowCount == 3;
        }

        #endregion

        public override ITableContext CreateTableContext(string tableName)
        {
            if (string.IsNullOrEmpty(tableName))
            {
                throw new ArgumentNullException("tableName");
            }

            return new PgTableContext(this, tableName);
        }

        public override void LockTable(string tableName)
        {
            if (string.IsNullOrEmpty(tableName))
            {
                throw new ArgumentNullException("tableName");
            }

            var sql = new SqlString("lock ", DataProvider.Dialect.QuoteForTableName(tableName));
            this.Execute(sql);
        }

    }
}
