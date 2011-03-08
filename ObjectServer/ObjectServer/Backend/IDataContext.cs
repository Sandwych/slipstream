using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;

namespace ObjectServer.Backend
{
    public interface IDataContext : IDisposable
    {

        void Open();
        void Close();

        void Initialize();

        bool IsInitialized();

        DbConnection Connection { get; }
        string DatabaseName { get; }
                
        object QueryValue(string commandText, params object[] args);
        DataTable QueryAsDataTable(string commandText, params object[] args);
        List<Dictionary<string, object>> QueryAsDictionary(string commandText, params object[] args);
        List<DynamicRecord> QueryAsDynamic(string commandText, params object[] args);

        int Execute(string commandText, params object[] args);

        ITableContext CreateTableContext(string tableName);

        long NextSerial(string sequenceName);
        bool IsValidDatabase();
        void LockTable(string tableName);
    }
}
