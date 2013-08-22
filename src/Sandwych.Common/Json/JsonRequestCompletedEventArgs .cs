using System;
using System.ComponentModel;


namespace Sandwych.Json
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
