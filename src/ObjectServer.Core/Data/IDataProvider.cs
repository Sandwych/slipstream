using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NHibernate.Dialect;
using NHibernate.Driver;

namespace ObjectServer.Data
{
    internal interface IDataProvider
    {
        IDBConnection CreateDataContext();
        IDBConnection CreateDataContext(string dbName);

        string[] ListDatabases();
        void CreateDatabase(string dbName);
        void DeleteDatabase(string dbName);

        Dialect Dialect { get; }
        IDriver Driver { get; }
    }
}
