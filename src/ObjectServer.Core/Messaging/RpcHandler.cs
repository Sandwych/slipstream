using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Diagnostics;

using ZMQ;
using ZMQ.ZMQExt;
using Newtonsoft.Json;

using ObjectServer.Exceptions;
using ObjectServer.Json;

namespace ObjectServer
{
    /// <summary>
    /// 处理 JSON-RPC 的 分发器
    /// </summary>
    public static class RpcHandler
    {
        private static Dictionary<string, MethodInfo> s_methods = new Dictionary<string, MethodInfo>();
        private static IExportedService s_service = Environment.ExportedService;

        static RpcHandler()
        {
            //注册自己的所有方法
            var selfType = typeof(RpcHandler);
            s_methods.Add("system.echo", selfType.GetMethod("Echo"));
            s_methods.Add("system.listMethods", selfType.GetMethod("ListMethods"));
            s_methods.Add("logOn", selfType.GetMethod("LogOn"));
            s_methods.Add("logOff", selfType.GetMethod("LogOff"));
            s_methods.Add("getVersion", selfType.GetMethod("GetVersion"));
            s_methods.Add("listDatabases", selfType.GetMethod("ListDatabases"));
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

        public static void LogOff(string sessionId)
        {
            s_service.LogOff(sessionId);
        }

        public static string GetVersion()
        {
            return s_service.GetVersion();
        }

        public static string[] ListDatabases()
        {
            return s_service.ListDatabases();
        }

        public static void DeleteDatabase(string hashedRootPassword, string dbName)
        {
            s_service.DeleteDatabase(hashedRootPassword, dbName);
        }

        public static object Execute(string sessionId, string objectName, string method, object[] parameters)
        {
            return s_service.Execute(sessionId, objectName, method, parameters);
        }

        #endregion

        public static void Start()
        {
            if (!Environment.Initialized)
            {
                throw new InvalidOperationException("无法启动 PRC-Handler 工人线程，请先初始化框架");
            }

            string rpcHandlerUrl = Environment.Configuration.RpcHandlerUrl;
            var id = Guid.NewGuid();
            LoggerProvider.RpcLogger.Info(() => string.Format("Starting RpcHandler Thread/Process, ID=[{0}] URL=[{1}] ...", id, rpcHandlerUrl));

            using (var receiver = new ZMQ.Socket(ZMQ.SocketType.REP))
            {
                receiver.Connect(rpcHandlerUrl);
                LoggerProvider.RpcLogger.Debug(() => string.Format("RpcHandler Thread/Process[{0}] connected to URL[{1}]", id, rpcHandlerUrl));
                while (true)
                {
                    //TODO 优化，避免转换
                    var message = receiver.Recv();
                    var result = DispatchJsonRpc(message);
                    receiver.Send(result);
                }
            }
        }

        private static byte[] DispatchJsonRpc(byte[] json)
        {
            Debug.Assert(json != null);

            var jreq = (Dictionary<string, object>)PlainJsonConvert.Parse(json);
            //TODO 检查 jreq 格式

            //执行调用
            var id = jreq[JsonRpcProtocol.Id];
            var methodName = (string)jreq[JsonRpcProtocol.Method];
            var method = s_methods[methodName];
            JsonRpcError error = null;
            object result = null;

            var args = (object[])jreq[JsonRpcProtocol.Params];

            LoggerProvider.RpcLogger.Debug(() =>
                string.Format("JSON-RPC: method=[{0}], params=[{1}]", methodName, args));

            //TODO: 处理安全问题及日志异常等
            //这里只捕获可控的异常
            try
            {
                result = method.Invoke(null, args);
            }
            catch (ArgumentException)
            {
                error = JsonRpcError.RpcArgumentError;
            }
            catch (ValidationException vex)
            {
                error = new JsonRpcError(JsonRpcError.ValidationError.Code, "数据验证错误", vex.Fields);
            }
            catch (FatalException fex)
            {
                LoggerProvider.RpcLogger.Fatal("系统发生了致命错误", fex);
                LoggerProvider.EnvironmentLogger.Fatal("系统发生了致命错误", fex);
                throw fex; //接着抛出异常，让系统结束运行
            }
            catch (System.Exception ex)
            {
                error = JsonRpcError.ServerInternalError;
                LoggerProvider.RpcLogger.Error("RPCHandler Error", ex);
            }

            var jresponse = new JsonRpcResponse()
            {
                Id = id,
                Error = error,
                Result = result
            };

            var jsonResponse = PlainJsonConvert.Generate(jresponse);
            return Encoding.UTF8.GetBytes(jsonResponse);
        }
    }
}
