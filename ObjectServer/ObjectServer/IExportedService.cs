using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer
{
    /// <summary>
    /// 对外服务接口
    /// </summary>
    public interface IExportedService
    {
        #region common services

        /// <summary>
        /// 用户登录系统
        /// </summary>
        /// <param name="dbName"></param>
        /// <param name="login"></param>
        /// <param name="password"></param>
        /// <returns>登录成功返回 session ID, 失败返回 null</returns>
        string LogOn(string dbName, string login, string password);

        void LogOff(string sessionId);

        string GetVersion();

        #endregion


        object Execute(
            string sessionId, string objectName, string name, params object[] parameters);

        string[] ListDatabases();
        void CreateDatabase(string rootPasswordHash, string dbName, string adminPassword);
        void DeleteDatabase(string rootPasswordHash, string dbName);

        long CreateModel(string dbName, string objectName, IDictionary<string, object> propertyBag);
        object[] SearchModel(string dbName, string objectName, object[] domain, long offset, long limit);
        Dictionary<string, object>[] ReadModel(string dbName, string objectName, object[] ids, object[] fields);
        void WriteModel(string dbName, string objectName, object id, IDictionary<string, object> record);
        void DeleteModel(string dbName, string objectName, object[] ids);
    }
}
