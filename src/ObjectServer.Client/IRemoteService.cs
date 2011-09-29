using System;
using System.Net;
using System.Collections.Generic;
using System.Threading.Tasks;

using ObjectServer.Client.Model;

namespace ObjectServer.Client
{
    public interface IRemoteService
    {

        void BeginGetVersion(Action<Version, Exception> resultCallback);
        void BeginListDatabases(Action<string[], Exception> resultCallback);
        Task<string[]> ListDatabasesAsync();
        void BeginCreateDatabase(string serverPasswordHash, string dbName, string adminPassword, Action resultCallback);
        void BeginDeleteDatabase(string serverPasswordHash, string dbName, Action resultCallback);

        void BeginLogOn(
           string dbName, string userName, string password, Action<string, Exception> resultCallback);

        void BeginLogOff(Action<Exception> resultCallback);

        void BeginExecute(
            string objectName, string method, object[] parameters, Action<object> resultCallback);
        void BeginExecute(
            string objectName, string method, object[] parameters, Action<object, Exception> resultCallback);
        Task<object> ExecuteAsync(
            string objectName, string method, object[] parameters);

        //辅助方法
        void CountModel(
            string objectName, object[][] constraints, Action<long, Exception> resultCallback);

        void SearchModel(
            string objectName, object[][] constraints,
            object[][] order, long offset, long limit, Action<long[], Exception> resultCallback);

        void ReadModel(
            string objectName, IEnumerable<long> ids, IEnumerable<string> fields,
            Action<Dictionary<string, object>[]> resultCallback);

        void CreateModel(
             string objectName, IDictionary<string, object> fields, Action<long> resultCallback);

        void WriteModel(string objectName, long id, IDictionary<string, object> fields, Action resultCallback);

        void DeleteModel(string objectName, long[] ids, Action resultCallback);

        void ReadAllMenus(Action<MenuModel[]> resultCallback);
    }
}
