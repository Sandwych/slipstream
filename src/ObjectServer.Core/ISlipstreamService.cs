using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer
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

        void LogOff(string db, string sessionId);

        //string[] GetResourceNames(string sessionId);

        string GetVersion();

        #endregion


        /// <summary>
        /// 执行资源的服务方法
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="resourceName"></param>
        /// <param name="name"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        object Execute(
            string db, string sessionId, string resourceName, string name, params object[] parameters);

        string[] ListDatabases();
        void CreateDatabase(string rootPasswordHash, string dbName, string adminPassword);
        void DeleteDatabase(string rootPasswordHash, string dbName);

        long CreateModel(string db, string sessionId, string modelName, IRecord propertyBag);
        long CountModel(string db, string sessionId, string modelName, object[] constraints = null);
        long[] SearchModel(string db, string sessionId, string modelName, object[] constraints = null,
            object[] order = null, long offset = 0, long limit = 0);
        Record[] ReadModel(string db, string sessionId, string modelName, object[] ids, object[] fields = null);
        void WriteModel(string db, string sessionId, string modelName, object id, IRecord record);
        void DeleteModel(string db, string sessionId, string modelName, object[] ids);
    }
}
