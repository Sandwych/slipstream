using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Diagnostics;

using ObjectServer.Model;

namespace ObjectServer.Backend
{
    internal abstract class DatabaseContextBase : IDatabaseContext
    {
        protected static readonly log4net.ILog Log = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        protected DbConnection conn;
        private bool opened = false;



        public void Open()
        {
            if (!this.opened)
            {
                this.conn.Open();
                this.opened = true;
            }
        }

        public void Close()
        {
            this.Dispose();
        }

        public void Delete(string dbName)
        {
            this.EnsureConnectionOpened();

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


        public DbConnection Connection { get { return this.conn; } }

        public string DatabaseName { get; protected set; }


        #region Query methods

        public virtual object QueryValue(string commandText, params object[] args)
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

        public virtual int Execute(string commandText, params object[] args)
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

        public virtual DataTable QueryAsDataTable(string commandText, params object[] args)
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

        public virtual List<Dictionary<string, object>> QueryAsDictionary(
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
                    this.opened = false;
                }
                this.conn.Dispose();
            }
        }

        #endregion



        protected DbCommand PrepareCommand(string commandText)
        {
            var command = this.conn.CreateCommand();
            command.CommandText = commandText;

            return command;
        }

        protected static void PrepareCommandParameters(DbCommand command, params object[] args)
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

        protected void EnsureConnectionOpened()
        {
            if (!this.opened)
            {
                this.Open();
            }
        }

        public string GetSqlType(IMetaField field)
        {
            throw new NotImplementedException();
        }

        public abstract string[] List();
        public abstract void Create(string dbName);
        public abstract void Initialize();
        public abstract ITableContext CreateTableHandler(IDatabaseContext db, string dbName);
        public abstract long NextSerial(string sequenceName);
    }
}
