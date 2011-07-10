using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

using ObjectServer.Client.Model;

namespace ObjectServer.Client
{
    public interface IRemoteService
    {
        void ListDatabases(Action<string[]> resultCallback);
        void LogOn(
           string dbName, string userName, string password, Action<string> resultCallback);
        void LogOff(Action resultCallback);
        void Execute(
            string objectName, string method, object[] parameters, Action<object> resultCallback);

        //辅助方法
        void ReadAllMenus(Action<MenuModel[]> resultCallback);
    }
}
