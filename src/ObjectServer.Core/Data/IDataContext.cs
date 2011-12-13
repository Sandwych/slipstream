using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;

using NHibernate.SqlCommand;

namespace ObjectServer.Data
{
    /// <summary>
    /// 核心数据库访问接口
    /// </summary>
    public interface IDataContext : IDisposable
    {
        void Close();

        void Setup();

        bool IsInitialized();

        string DatabaseName { get; }

        object QueryValue(SqlString commandText, params object[] args);
        DataTable QueryAsDataTable(SqlString commandText, params object[] args);
        Dictionary<string, object>[] QueryAsDictionary(SqlString commandText, params object[] args);
        dynamic[] QueryAsDynamic(SqlString commandText, params object[] args);
        T[] QueryAsArray<T>(SqlString commandText, params object[] args);
        IDataReader QueryAsReader(SqlString commandText, params object[] args);
        int Execute(SqlString commandText, params object[] args);

        ITableContext CreateTableContext(string tableName);

        long GetLastIdentity(string sequenceName);
        bool IsValidDatabase();
        void LockTable(string tableName);

        IDbCommand CreateCommand(SqlString sql);
        IDbTransaction Transaction { get; }
        IDbTransaction BeginTransaction();
    }
}
