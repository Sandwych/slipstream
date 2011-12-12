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
    internal sealed class PgDataContext : AbstractDataContext, IDataContext
    {
        private readonly static Type pgt = typeof(Npgsql.NpgsqlCommand);

        private const string INITDB = "ObjectServer.Data.Postgresql.initdb.sql";

        public PgDataContext(string dbName)
        {
            if (string.IsNullOrEmpty(dbName))
            {
                throw new ArgumentNullException("_dbName");
            }

            var cfg = SlipstreamEnvironment.Configuration;
            string connectionString = string.Format(
              CultureInfo.InvariantCulture,
              "Server={0};" +
              "Database={3};" +
              "Encoding=UNICODE;" +
              "User ID={1};" +
              "Password={2};",
              cfg.DbHost, cfg.DbUser, cfg.DbPassword, dbName);
            var dbc = new Npgsql.NpgsqlConnection(connectionString);
            dbc.Open();
            //this.conn = new NpgsqlConnection(connectionString);
            this._conn = dbc;
            this.DatabaseName = dbName;
        }

        public PgDataContext()
            : this("template1")
        {
        }

        #region IDatabase 成员

        public override void Create(string dbName)
        {
            if (string.IsNullOrEmpty(dbName))
            {
                throw new ArgumentNullException("_dbName");
            }

            LoggerProvider.EnvironmentLogger.Info(String.Format("Creating Database [{0}]...", dbName));

            var sql = string.Format(CultureInfo.InvariantCulture,
                @"create database ""{0}"" template ""template0"" encoding 'unicode' ", dbName);

            this.Execute(SqlString.Parse(sql));

            LoggerProvider.EnvironmentLogger.Info(
                String.Format("Database [{0}] has been created.", dbName));
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
            LoggerProvider.EnvironmentLogger.Info("The database scheme has been _initialized.");
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

        public override long GetLastIdentity(string tableName)
        {
            if (string.IsNullOrEmpty(tableName))
            {
                throw new ArgumentNullException("tableName");
            }

            var sql = string.Format(CultureInfo.InvariantCulture,
                @"select currval('{0}__id_seq')", tableName);

            return Convert.ToInt64(this.QueryValue(new SqlString(sql)));
        }

    }
}
