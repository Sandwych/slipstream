using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ObjectServer.Backend;

namespace ObjectServer
{
    public class Database : IDatabase
    {
        public Database(string dbName)
        {
            this.DataContext = DataProvider.CreateDataContext(dbName);
            this.Objects = new ObjectCollection(this);
        }

        public IDataContext DataContext { get; private set; }

        public IObjectCollection Objects { get; private set; }

        #region IDisposable 成员

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IGlobalObject 成员

        public void Initialize(Config cfg)
        {
            this.Objects.Initialize();
        }

        #endregion
    }
}
