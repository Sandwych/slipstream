using System;
using System.Net;
using System.Collections.Generic;

using ObjectServer.Client.Model;

namespace ObjectServer.Client
{
    public interface IRootService
    {

        void GetVersion(Action<Version, Exception> resultCallback);
        void ListDatabases(Action<string[], Exception> resultCallback);
        void CreateDatabase(
            string serverPasswordHash, string dbName, string adminPassword, Action<Exception> resultCallback);
        void DeleteDatabase(string serverPasswordHash, string dbName, Action<Exception> resultCallback);

        void LogOn(
           string dbName, string userName, string password, Action<string, Exception> resultCallback);

        void LogOff(Action<Exception> resultCallback);

        void Execute(
            string objectName, string method, object[] parameters, Action<object, Exception> resultCallback);

        //辅助方法
        void CountModel(
            string objectName, object[][] constraints, Action<long, Exception> resultCallback);

        void SearchModel(
            string objectName, object[][] constraints,
            object[][] order, long offset, long limit, Action<long[], Exception> resultCallback);

        void ReadModel(
            string objectName, IEnumerable<long> ids, IEnumerable<string> fields,
            Action<Dictionary<string, object>[], Exception> resultCallback);

        void CreateModel(
             string objectName, IDictionary<string, object> fields, Action<long, Exception> resultCallback);

        void WriteModel(string objectName, long id, IDictionary<string, object> fields, Action<Exception> resultCallback);

        void DeleteModel(string objectName, long[] ids, Action<Exception> resultCallback);

        void ReadAllMenus(Action<MenuEntity[], Exception> resultCallback);
    }
}
