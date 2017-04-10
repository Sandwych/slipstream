using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SlipStream
{
    using IRecord = IDictionary<string, object>;
    using Record = Dictionary<string, object>;

    /// <summary>
    /// 核心对外服务接口
    /// </summary>
    public interface ISlipstreamService
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

        void LogOff(string db, string sessionToken);

        //string[] GetResourceNames(string sessionToken);

        string GetVersion();

        #endregion


        /// <summary>
        /// 执行资源的服务方法
        /// </summary>
        /// <param name="sessionToken"></param>
        /// <param name="resourceName"></param>
        /// <param name="name"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        object Execute(
            string db, string sessionToken, string resourceName, string name, params object[] parameters);

        string[] ListDatabases();
        void CreateDatabase(string rootPasswordHash, string dbName, string adminPassword);
        void DeleteDatabase(string rootPasswordHash, string dbName);

        long CreateEntity(string db, string sessionToken, string entityName, IRecord propertyBag);
        long CountEntity(string db, string sessionToken, string entityName, object[] constraints = null);
        long[] SearchEntity(string db, string sessionToken, string entityName, object[] constraints = null,
            object[] order = null, long offset = 0, long limit = 0);
        Record[] ReadEntity(string db, string sessionToken, string entityName, object[] ids, object[] fields = null);
        void WriteEntity(string db, string sessionToken, string entityName, object id, IRecord record);
        void DeleteEntity(string db, string sessionToken, string entityName, object[] ids);
    }
}
