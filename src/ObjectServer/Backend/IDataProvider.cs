using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.Backend
{
    internal interface IDataProvider
    {
        IDBConnection CreateDataContext();
        IDBConnection CreateDataContext(string dbName);

        string[] ListDatabases();
        void CreateDatabase(string dbName);
        void DeleteDatabase(string dbName);

    }
}
