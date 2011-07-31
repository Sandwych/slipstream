using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Diagnostics;

using ObjectServer.Model;

namespace ObjectServer.Data
{
    internal abstract class AbstractDBConnection : IDBConnection
    {
        protected DbConnection conn;
        private bool opened;

        public AbstractDBConnection()
        {
            this.opened = false;
        }

        ~AbstractDBConnection()
        {
            this.Dispose(false);
        }

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
            if (this.opened)
            {
                this.conn.Close();
                this.opened = false;
            }
        }

        public void Delete(string dbName)
        {
            if (string.IsNullOrEmpty(dbName))
            {
                throw new ArgumentNullException("dbName");
            }

            this.EnsureConnectionOpened();

            var sql = string.Format(
                "DROP DATABASE \"{0}\"", dbName);

            Logger.Debug(() => "SQL: " + sql);

            var cmd = this.DBConnection.CreateCommand();
            cmd.CommandText = sql;
            cmd.ExecuteNonQuery();
        }


        public DbConnection DBConnection { get { return this.conn; } }

        public string DatabaseName { get; protected set; }


        #region Query methods

        public virtual object QueryValue(string commandText, params object[] args)
        {
            if (string.IsNullOrEmpty(commandText))
            {
                throw new ArgumentNullException("commandText");
            }

            this.EnsureConnectionOpened();

            Logger.Debug(() => "SQL: " + commandText);

            using (var cmd = PrepareCommand(commandText))
            {
                PrepareCommandParameters(cmd, args);
                var result = cmd.ExecuteScalar();
                return result;
            }
        }

        public virtual int Execute(string commandText, params object[] args)
        {
            if (string.IsNullOrEmpty(commandText))
            {
                throw new ArgumentNullException("commandText");
            }

            this.EnsureConnectionOpened();

            Logger.Debug(() => "SQL: " + commandText);

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
            if (string.IsNullOrEmpty(commandText))
            {
                throw new ArgumentNullException("commandText");
            }

            this.EnsureConnectionOpened();

            Logger.Debug(() => ("SQL: " + commandText));

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


        public virtual Dictionary<string, object>[] QueryAsDictionary(
            string commandText, params object[] args)
        {
            if (string.IsNullOrEmpty(commandText))
            {
                throw new ArgumentNullException("commandText");
            }

            EnsureConnectionOpened();

            Logger.Debug(() => "SQL: " + commandText);

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

                return rows.ToArray();
            }
        }

        public virtual dynamic[] QueryAsDynamic(string commandText, params object[] args)
        {
            if (string.IsNullOrEmpty(commandText))
            {
                throw new ArgumentNullException("commandText");
            }

            var dicts = QueryAsDictionary(commandText, args);

            return dicts.Select(r => new DynamicRecord(r)).ToArray();
        }


        public virtual T[] QueryAsArray<T>(string commandText, params object[] args)
        {
            if (string.IsNullOrEmpty(commandText))
            {
                throw new ArgumentNullException("commandText");
            }

            this.EnsureConnectionOpened();

            Logger.Debug(() => ("SQL: " + commandText));

            using (var command = PrepareCommand(commandText))
            {
                PrepareCommandParameters(command, args);
                using (var reader = command.ExecuteReader())
                {
                    var result = new List<T>();
                    while (reader.Read())
                    {
                        result.Add((T)reader[0]);
                    }
                    return result.ToArray();
                }
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
                this.conn.Close();
            }

            this.conn.Dispose();
        }

        #endregion



        protected DbCommand PrepareCommand(string commandText)
        {
            Debug.Assert(!string.IsNullOrEmpty(commandText));

            var command = this.conn.CreateCommand();
            command.CommandText = commandText;

            return command;
        }

        protected static void PrepareCommandParameters(DbCommand command, params object[] args)
        {
            Debug.Assert(command != null);

            if (args == null || args.Length == 0)
            {
                return;
            }

            for (int i = 0; i < args.Length; i++)
            {
                var param = command.CreateParameter();
                param.ParameterName = "@" + i.ToString();
                param.Value = args[i];
                command.Parameters.Add(param);
            }
        }

        protected void EnsureConnectionOpened()
        {
            this.Open();
        }

        public abstract bool IsInitialized();
        public abstract string[] List();
        public abstract void Create(string dbName);
        public abstract void Initialize();
        public abstract ITableContext CreateTableContext(string tableName);
        public abstract long NextSerial(string sequenceName);
        public abstract void LockTable(string tableName);

        public virtual bool IsValidDatabase()
        {
            throw new NotImplementedException();
        }
    }
}
