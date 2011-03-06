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

namespace ObjectServer.Client
{
    internal class RequestState
    {
        public HttpWebRequest HttpRequest { get; private set; }
        public HttpWebResponse HttpResponse { get; set; }
        public JsonRpcResponse JsonResponse { get; set; }
        public ResultCallbackHandler ResultHandler { get; private set; }

        public RequestState(HttpWebRequest httpRequest, ResultCallbackHandler rh)
        {
            this.HttpRequest = httpRequest;
            this.ResultHandler = rh;

            this.HttpResponse = null;
            this.JsonResponse = null;
        }
    }
}
