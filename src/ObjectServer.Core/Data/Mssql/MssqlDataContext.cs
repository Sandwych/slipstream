using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using System.Globalization;

using NHibernate.SqlCommand;

namespace ObjectServer.Data.Mssql
{
    internal sealed class MssqlDataContext : AbstractDataContext, IDataContext
    {
        private readonly static Type sbc = typeof(System.Data.SqlClient.SqlBulkCopy);

        private const string INITDB = "ObjectServer.Data.Mssql.initdb.sql";

        public MssqlDataContext(string dbName)
        {
            if (string.IsNullOrEmpty(dbName))
            {
                throw new ArgumentNullException("dbName");
            }
            var cfg = Environment.Configuration;
            string connectionString = string.Format(
              CultureInfo.InvariantCulture,
              "Data Source={0};" +
              "Initial Catalog={3};" +
              "User Id={1};" +
              "Password={2};",
              cfg.DbHost, cfg.DbUser, cfg.DbPassword, dbName);
            var dbc = DataProvider.Driver.CreateConnection();
            dbc.ConnectionString = connectionString;
            this._conn = dbc;
            dbc.Open();
            this.DatabaseName = dbName;
        }

        public MssqlDataContext()
            : this("master")
        {
        }

        #region IDatabase 成员

        public override void Create(string dbName)
        {
            if (string.IsNullOrEmpty(dbName))
            {
                throw new ArgumentNullException("dbName");
            }

            LoggerProvider.EnvironmentLogger.Info(String.Format("Creating Database [{0}]...", dbName));

            var sqlBuilder = new SqlStringBuilder();
            sqlBuilder.Add("create database ");
            sqlBuilder.Add('"' + dbName + '"');
            sqlBuilder.Add(";");

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
            var rowCount = Convert.ToInt32(this.QueryValue(sql));
            return rowCount == 3;
        }

        #endregion

        public override ITableContext CreateTableContext(string tableName)
        {
            if (string.IsNullOrEmpty(tableName))
            {
                throw new ArgumentNullException("tableName");
            }

            return new MssqlTableContext(this, tableName);
        }

        public override void LockTable(string tableName)
        {
            if (string.IsNullOrEmpty(tableName))
            {
                throw new ArgumentNullException("tableName");
            }

            //TODO
            //SQL Server 的锁需要研究一下
            //var sql = new SqlString("lock ", DataProvider.Dialect.QuoteForTableName(tableName));
            //this.Execute(sql);
        }

        public override long GetLastIdentity(string tableName)
        {
            if (string.IsNullOrEmpty(tableName))
            {
                throw new ArgumentNullException("tableName");
            }

            var sql = new SqlString("SELECT CAST(IDENT_CURRENT('", tableName, "') AS BIGINT)");
            var value = this.QueryValue(sql);
            return (long)value;
        }

    }
}
