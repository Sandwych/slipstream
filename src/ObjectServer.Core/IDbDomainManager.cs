using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer
{
    public interface IDbDomainManager : IDisposable
    {
        void Register(string dbName, bool isUpdate);
        IDbDomain GetDbDomain(string dbName);
        void Remove(string dbName);
    }
}
