using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Diagnostics;
using System.Threading;

using Newtonsoft.Json;

using SlipStream.Exceptions;
using Sandwych.Json;

namespace SlipStream.Server
{
    /// <summary>
    /// 处理 JSON-RPC 的 分发器
    /// </summary>
    public static class ServiceDispatcher
    {
        private static readonly Dictionary<string, MethodInfo> s_methods = new Dictionary<string, MethodInfo>();
        private static readonly ISlipstreamService s_service = SlipstreamEnvironment.RootService;
        private static readonly object s_lockObj = new object();

        static ServiceDispatcher()
        {
            //注册自己的所有方法
            var selfType = typeof(ServiceDispatcher);
            s_methods.Add("system.echo", selfType.GetMethod("Echo"));
            s_methods.Add("system.listMethods", selfType.GetMethod("ListMethods"));

            s_methods.Add("logOn", selfType.GetMethod("LogOn"));
            s_methods.Add("logOff", selfType.GetMethod("LogOff"));
            s_methods.Add("getVersion", selfType.GetMethod("GetVersion"));
            s_methods.Add("listDatabases", selfType.GetMethod("ListDatabases"));
            s_methods.Add("createDatabase", selfType.GetMethod("CreateDatabase"));
            s_methods.Add("deleteDatabase", selfType.GetMethod("DeleteDatabase"));
            s_methods.Add("execute", selfType.GetMethod("Execute"));
        }

        #region JSON-RPC system methods

        public static string[] ListMethods()
        {
            return s_methods.Keys.ToArray();
        }

        public static object Echo(object value)
        {
            return value;
        }

        #endregion

        #region 业务 JSON-RPC 方法

        public static string LogOn(string dbName, string userName, string password)
        {
            return s_service.LogOn(dbName, userName, password);
        }

        public static void LogOff(string db, string sessionToken)
        {
            s_service.LogOff(db, sessionToken);
        }

        public static string GetVersion()
        {
            return s_service.GetVersion();
        }

        public static string[] ListDatabases()
        {
            return s_service.ListDatabases();
        }

        public static void CreateDatabase(string hashedRootPassword, string dbName, string adminPassword)
        {
            s_service.CreateDatabase(hashedRootPassword, dbName, adminPassword);
        }

        public static void DeleteDatabase(string hashedRootPassword, string dbName)
        {
            s_service.DeleteDatabase(hashedRootPassword, dbName);
        }

        public static object Execute(string db, string sessionToken, string objectName, string method, object[] parameters)
        {
            return s_service.Execute(
                db, sessionToken, objectName, method, parameters);
        }

        #endregion

        public static byte[] InvokeJsonRpc(byte[] json)
        {
            Debug.Assert(json != null);

            var jreq = (IDictionary<string, object>)PlainJsonConvert.Parse(json);

            //执行调用
            object id;
            if (!jreq.TryGetValue(JsonRpcProtocol.Id, out id))
            {
                return GenerateResponse(new JsonRpcResponse() { Error = JsonRpcError.InvalidRequest, });
            }

            object methodNameObj;
            if (!jreq.TryGetValue(JsonRpcProtocol.Method, out methodNameObj))
            {
                return GenerateResponse(new JsonRpcResponse()
                {
                    Id = id,
                    Error = JsonRpcError.InvalidRpcMethod
                });
            }
            string methodName = (string)methodNameObj;

            MethodInfo method;
            if (!s_methods.TryGetValue(methodName, out method) || method == null)
            {
                return GenerateResponse(new JsonRpcResponse()
                {
                    Id = id,
                    Error = JsonRpcError.InvalidRpcMethod
                });
            }

            JsonRpcError error = null;
            object result = null;

            object argsObj;
            if (!jreq.TryGetValue(JsonRpcProtocol.Params, out argsObj))
            {
                return GenerateResponse(new JsonRpcResponse()
                {
                    Id = id,
                    Error = JsonRpcError.InvalidRpcMethod
                });
            }
            var args = (object[])argsObj;

            LoggerProvider.RpcLogger.Debug(() =>
                string.Format("JSON-RPC: method=[{0}], params=[{1}]", methodName, args));

            try
            {
                var startTime = Stopwatch.GetTimestamp();

                try
                {
                    result = method.Invoke(null, args);
                }
                catch (TargetInvocationException tiex)
                {
                    throw tiex.InnerException;
                }
                catch
                {
                    throw;
                }

                var endTime = Stopwatch.GetTimestamp();
                var costTime = endTime - startTime;
                LoggerProvider.RpcLogger.Debug(
                    () => String.Format("Transaction costed time: [{0:N0}ms]", costTime * 1000 / Stopwatch.Frequency));
            }
            catch (FatalException fex)
            {
                LoggerProvider.EnvironmentLogger.FatalException("FatalError", fex);
                throw fex; //接着抛出异常，让系统结束运行
            }
            catch (ArgumentException ex)
            {
                error = JsonRpcError.RpcArgumentError;
                LoggerProvider.EnvironmentLogger.ErrorException("ArgumentException", ex);
            }
            catch (ValidationException ex)
            {
                error = JsonRpcError.ValidationError;
                LoggerProvider.EnvironmentLogger.ErrorException("ValidationException", ex);
            }
            catch (SecurityException ex)
            {
                error = JsonRpcError.SecurityError;
                LoggerProvider.EnvironmentLogger.ErrorException("SecurityError", ex);
            }
            catch (ResourceNotFoundException ex)
            {
                error = JsonRpcError.ResourceNotFound;
                LoggerProvider.EnvironmentLogger.ErrorException("ResourceNotFoundException", ex);
            }
            catch (RecordNotFoundException ex)
            {
                error = JsonRpcError.ResourceNotFound;
                LoggerProvider.EnvironmentLogger.ErrorException("ResourceNotFoundException", ex);
            }
            catch (DataException ex)
            {
                error = JsonRpcError.BadData;
                LoggerProvider.EnvironmentLogger.ErrorException("BadData", ex);
            }
            catch (System.Data.Common.DbException ex)
            {
                error = JsonRpcError.DBError;
                LoggerProvider.EnvironmentLogger.ErrorException("DBError", ex);
            }
            catch (System.Exception ex)
            {
                error = JsonRpcError.ServerInternalError;
                LoggerProvider.EnvironmentLogger.ErrorException("RPCHandler Error", ex);
                throw ex; //未知异常，与致命异常同样处理，直接抛出，让系统结束运行
            }

            var jresponse = new JsonRpcResponse()
            {
                Id = id,
                Error = error,
                Result = result
            };

            return GenerateResponse(jresponse);
        }

        private static byte[] GenerateResponse(JsonRpcResponse jresponse)
        {
            var jsonResponse = PlainJsonConvert.Generate(jresponse);
            return Encoding.UTF8.GetBytes(jsonResponse);
        }
    }
}
