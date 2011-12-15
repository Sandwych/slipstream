using System;
using System.ComponentModel;


namespace ObjectServer.Json
{
    public sealed class JsonRequestCompletedEventArgs : AsyncCompletedEventArgs
    {
        public JsonRequestCompletedEventArgs(JsonRpcResponse result, Exception error, object userState)
            : base(error, false, userState)
        {
            this.Result = result;
        }

        public JsonRpcResponse Result { get; private set; }
    }
}
