﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;

using NHibernate.SqlCommand;

namespace ObjectServer.Data
{
    public interface IDBConnection : IDisposable
    {

        void Open();
        void Close();

        void Initialize();

        bool IsInitialized();

        DbConnection DBConnection { get; }
        string DatabaseName { get; }
                
        object QueryValue(string commandText, params object[] args);
        DataTable QueryAsDataTable(SqlString commandText, params object[] args);
        Dictionary<string, object>[] QueryAsDictionary(SqlString commandText, params object[] args);
        dynamic[] QueryAsDynamic(SqlString commandText, params object[] args);
        T[] QueryAsArray<T>(SqlString commandText, params object[] args);

        int Execute(string commandText, params object[] args);

        ITableContext CreateTableContext(string tableName);

        long NextSerial(string sequenceName);
        bool IsValidDatabase();
        void LockTable(string tableName);

        IDbCommand CreateCommand(NHibernate.SqlCommand.SqlString sql);
    }
}
