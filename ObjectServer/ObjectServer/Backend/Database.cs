using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Diagnostics;

using Npgsql;

using ObjectServer.Model;
using ObjectServer.Model.Fields;

namespace ObjectServer.Backend
{
    public class Database : IDisposable
    {
        protected static readonly log4net.ILog Log = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private DbConnection conn;
        private bool opened;

        public Database(string dbName)
        {
            string connectionString =
              "Server=localhost;" +
              "Database=objectserver;" +
              "Encoding=UNICODE;" +
              "User ID=objectserver;" +
              "Password=objectserver;";
            this.conn = new NpgsqlConnection(connectionString);
            this.DatabaseName = dbName;
        }

        public Database()
            : this("template1")
        {
        }

        public void Open()
        {
            this.conn.Open();
            this.opened = true;
        }

        public void Close()
        {
            if (this.Connected)
            {
                this.conn.Close();
            }
        }

        public void Create(string dbName)
        {
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

        public void Delete(string dbName)
        {
            var sql = string.Format(
                "DROP DATABASE \"{0}\"", dbName);

            if (Log.IsDebugEnabled)
            {
                Log.Info(sql);
            }

            var cmd = this.Connection.CreateCommand();
            cmd.CommandText = sql;
            cmd.ExecuteNonQuery();
        }


        public string[] List()
        {
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

        public DbConnection Connection { get { return this.conn; } }

        public bool Connected
        {
            get
            {
                return this.conn.State != ConnectionState.Closed &&
                    this.conn.State != ConnectionState.Broken;
            }
        }

        public string DatabaseName { get; private set; }


        #region Query methods

        public object QueryValue(string commandText, params object[] args)
        {
            this.EnsureConnectionOpened();

            if (Log.IsDebugEnabled)
            {
                Log.Info(commandText);
            }

            using (var cmd = PrepareCommand(commandText))
            {
                PrepareCommandParameters(cmd, args);
                var result = cmd.ExecuteScalar();
                return result;
            }
        }

        public int Execute(string commandText, params object[] args)
        {
            this.EnsureConnectionOpened();

            if (Log.IsDebugEnabled)
            {
                Log.Info(commandText);
            }

            using (var command = PrepareCommand(commandText))
            {
                PrepareCommandParameters(command, args);
                EnsureConnectionOpened();
                var result = command.ExecuteNonQuery();
                return result;
            }
        }

        public DataTable QueryAsDataTable(string commandText, params object[] args)
        {
            this.EnsureConnectionOpened();

            if (Log.IsDebugEnabled)
            {
                Log.Info(commandText);
            }

            using (var command = PrepareCommand(commandText))
            {
                PrepareCommandParameters(command, args);
                using (var reader = command.ExecuteReader())
                {
                    var tb = new DataTable();
                    while (reader.Read())
                    {
                        for (int i = 0; i < reader.FieldCount; ++i)
                        {
                            var columnName = reader.GetName(i);
                            if (!tb.Columns.Contains(columnName))
                            {
                                tb.Columns.Add(columnName);
                            }
                        }
                        var row = tb.NewRow();
                        for (int i = 0; i < reader.FieldCount; ++i)
                        {
                            row[i] = reader[i];
                        }

                        tb.Rows.Add(row);

                    }
                    return tb;
                }
            }
        }

        public List<Dictionary<string, object>> QueryAsDictionary(
            string commandText, params object[] args)
        {
            EnsureConnectionOpened();

            if (Log.IsDebugEnabled)
            {
                Log.Info(commandText);
            }

            using (var command = PrepareCommand(commandText))
            {
                if (args != null && args.Length > 0)
                {
                    PrepareCommandParameters(command, args);
                }
                var rows = new List<Dictionary<string, object>>();

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var fields = new Dictionary<string, object>();
                        for (int i = 0; i < reader.FieldCount; ++i)
                        {
                            var fieldName = reader.GetName(i);
                            fields[fieldName] = reader.GetValue(i);
                        }
                        rows.Add(fields);
                    }
                }

                return rows;
            }
        }

        #endregion

        #region IDisposable 成员

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.opened)
                {
                    this.conn.Close();
                }
                this.conn.Dispose();
            }
        }

        #endregion



        private DbCommand PrepareCommand(string commandText)
        {
            var command = this.conn.CreateCommand();
            command.CommandText = commandText;

            return command;
        }

        private static void PrepareCommandParameters(DbCommand command, params object[] args)
        {
            if (args == null || args.Length == 0)
            {
                return;
            }

            int index = 0;

            foreach (var arg in args)
            {
                var param = command.CreateParameter();
                param.ParameterName = "@" + index;
                param.Value = args[index++];
                command.Parameters.Add(param);
            }
        }

        private void EnsureConnectionOpened()
        {
            if (this.opened)
            {
                return;
            }

            this.conn.Open();
        }

        public string GetSqlType(IField field)
        {
            throw new NotImplementedException();
        }
    }
}
