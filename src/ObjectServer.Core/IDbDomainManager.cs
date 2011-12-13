using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer
{
    public interface IDbDomainManager : IDisposable
    {
        void Initialize(Config cfg);
        void Register(string dbName, bool isUpdate);
        IDbDomain GetDbProfile(string dbName);
        void Remove(string dbName);
    }
}
