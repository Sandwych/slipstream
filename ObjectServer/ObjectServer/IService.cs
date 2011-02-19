using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer
{
    public interface IService
    {
        Guid Login(string dbName, string username, string password);
        void Logout(string dbName, Guid session);


        object Execute(
            string dbName, string objectName, string name, params object[] args);

        string[] ListDatabases();
        void CreateDatabase(string rootPasswordHash, string dbName, string adminPassword);
        void DeleteDatabase(string rootPasswordHash, string dbName);
    }
}
