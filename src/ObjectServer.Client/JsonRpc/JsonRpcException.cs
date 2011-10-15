using System;

namespace ObjectServer.Client
{
    public class JsonRpcException : Exception
    {
        public JsonRpcException(string msg, JsonRpcError error)
            : base(msg)
        {
            this.Error = error;
        }

        public JsonRpcError Error { get; private set; }

        public override string ToString()
        {
            return this.Error.ToString();
        }
    }
}
