using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Diagnostics;

using Autofac;
using NHibernate.SqlCommand;

using SlipStream.Entity;

namespace SlipStream.Data {
    internal abstract class AbstractDataContext : IDataContext {
        protected IDbConnection _conn;
        protected bool _disposed = false;
        protected IDbTransaction _transaction;

        public AbstractDataContext() {
        }

        ~AbstractDataContext() {
            this.Dispose(false);
        }

        public void Close() {
            this._conn.Close();
        }

        public string DatabaseName { get; protected set; }

        #region Query methods

        public virtual object QueryValue(SqlString commandText, params object[] args) {
            if (commandText == null) {
                throw new ArgumentNullException("commandText");
            }

            if (SlipstreamEnvironment.Settings.LoggingSql) {
                LoggerProvider.EnvironmentLogger.Debug(() => "SQL: " + commandText);
            }

            using (var command = this.CreateCommand(commandText)) {
                PrepareNamedParameters(command, args);
                var result = command.ExecuteScalar();
                return result;
            }
        }

        public virtual int Execute(SqlString commandText, params object[] args) {
            if (commandText == null) {
                throw new ArgumentNullException("commandText");
            }

            if (SlipstreamEnvironment.Settings.LoggingSql) {
                LoggerProvider.EnvironmentLogger.Debug(() => "SQL: " + commandText);
            }

            using (var command = this.CreateCommand(commandText)) {
                PrepareNamedParameters(command, args);
                var result = command.ExecuteNonQuery();
                return result;
            }
        }

        public virtual DataTable QueryAsDataTable(SqlString commandText, params object[] args) {
            if (commandText == null) {
                throw new ArgumentNullException("commandText");
            }

            using (var reader = this.QueryAsReader(commandText, args)) {
                var tb = new DataTable();
                while (reader.Read()) {
                    for (int i = 0; i < reader.FieldCount; ++i) {
                        var columnName = reader.GetName(i);
                        if (!tb.Columns.Contains(columnName)) {
                            tb.Columns.Add(columnName);
                        }
                    }
                    var row = tb.NewRow();
                    for (int i = 0; i < reader.FieldCount; ++i) {
                        row[i] = reader[i];
                    }

                    tb.Rows.Add(row);
                }

                return tb;
            }

        }

        public virtual Dictionary<string, object>[] QueryAsDictionary(
            SqlString commandText, params object[] args) {
            if (commandText == null) {
                throw new ArgumentNullException("commandText");
            }

            var rows = new List<Dictionary<string, object>>();

            using (var reader = this.QueryAsReader(commandText, args)) {
                while (reader.Read()) {
                    var fields = new Dictionary<string, object>();
                    for (int i = 0; i < reader.FieldCount; ++i) {
                        var fieldName = reader.GetName(i);
                        fields[fieldName] = reader.GetValue(i);
                    }
                    rows.Add(fields);
                }
            }

            return rows.ToArray();
        }

        public virtual dynamic[] QueryAsDynamic(SqlString commandText, params object[] args) {
            if (commandText == null) {
                throw new ArgumentNullException("commandText");
            }

            var dicts = QueryAsDictionary(commandText, args);

            return dicts.Select(r => new DynamicRecord(r)).ToArray();
        }


        public virtual T[] QueryAsArray<T>(SqlString commandText, params object[] args) {
            if (commandText == null) {
                throw new ArgumentNullException("commandText");
            }

            using (var reader = this.QueryAsReader(commandText, args)) {
                var result = new List<T>();
                while (reader.Read()) {
                    result.Add((T)reader[0]);
                }
                return result.ToArray();
            }
        }

        public virtual IDataReader QueryAsReader(SqlString commandText, params object[] args) {
            if (commandText == null) {
                throw new ArgumentNullException("commandText");
            }

            if (SlipstreamEnvironment.Settings.LoggingSql) {
                LoggerProvider.EnvironmentLogger.Debug(() => ("SQL: " + commandText));
            }

            using (var command = this.CreateCommand(commandText)) {
                PrepareNamedParameters(command, args);
                return command.ExecuteReader();
            }
        }


        #endregion


        #region Query methods with plain sql string

        public virtual object QueryValue(string commandText, params object[] args) {
            return this.QueryValue(SqlString.Parse(commandText), args);
        }

        public virtual int Execute(string commandText, params object[] args) {
            return this.Execute(SqlString.Parse(commandText), args);
        }

        public virtual DataTable QueryAsDataTable(string commandText, params object[] args) {
            return this.QueryAsDataTable(SqlString.Parse(commandText), args);
        }

        public virtual Dictionary<string, object>[] QueryAsDictionary(
            string commandText, params object[] args) {
            return this.QueryAsDictionary(SqlString.Parse(commandText), args);
        }

        public virtual dynamic[] QueryAsDynamic(string commandText, params object[] args) {
            return this.QueryAsDynamic(SqlString.Parse(commandText), args);
        }


        public virtual T[] QueryAsArray<T>(string commandText, params object[] args) {
            return this.QueryAsArray<T>(SqlString.Parse(commandText), args);
        }

        public virtual IDataReader QueryAsReader(string commandText, params object[] args) {
            return this.QueryAsReader(SqlString.Parse(commandText), args);
        }


        #endregion

        #region IDisposable 成员

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            if (!this._disposed) {
                //释放非托管资源
                if (disposing) {
                }

                //释放托管资源
                this.Close();
                this._conn.Dispose();

                this._disposed = true;
            }
        }

        #endregion

        protected IDbCommand PrepareCommand(string commandText) {
            Debug.Assert(!string.IsNullOrEmpty(commandText));
            Debug.Assert(!this._disposed);

            var command = this._conn.CreateCommand();
            command.CommandText = commandText;

            return command;
        }

        public abstract bool IsInitialized();
        public abstract void Create(string dbName);
        public abstract void Setup();
        public abstract ITableContext CreateTableContext(string tableName);
        public abstract void LockTable(string tableName);
        public abstract long GetLastIdentity(string sequenceName);

        public virtual bool IsValidDatabase() {
            throw new NotImplementedException();
        }

        public IDbTransaction Transaction {
            get {
                if (this._transaction == null) {
                    this._transaction = _conn.BeginTransaction();
                }
                return this._transaction;
            }
        }

        public virtual IDbTransaction BeginTransaction() {
            return this.Transaction;
        }

        public IDbCommand CreateCommand(SqlString sql) {
            //TODO 改成依赖的
            var dataProvider = SlipstreamEnvironment.RootContainer.Resolve<IDataProvider>();
            var sqlCommand = dataProvider.Driver.GenerateCommand(
                CommandType.Text, sql, new NHibernate.SqlTypes.SqlType[] { });
            sqlCommand.Connection = this._conn;
            if (this._transaction != null) {
                sqlCommand.Transaction = this._transaction;
            }

            return sqlCommand;
        }

        public abstract IDbCommand CreateCommand(string sql);

        private static void PrepareNamedParameters(IDbCommand sqlCommand, object[] args) {
            Debug.Assert(args != null);
            Debug.Assert(sqlCommand != null);

            for (int i = 0; i < args.Length; i++) {
                var value = args[i];
                var param = sqlCommand.CreateParameter();
                param.ParameterName = 'p' + i.ToString();
                if (value is SqlCommandParameter) {
                    var scp = value as SqlCommandParameter;
                    param.Value = value == null ? DBNull.Value : scp.Value;
                    param.DbType = scp.Type;
                } 
                else {
                    param.Value = value == null ? DBNull.Value : value;
                }
                sqlCommand.Parameters.Add(param);
            }
        }
    }
}
