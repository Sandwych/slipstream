using System;

namespace Malt.Json
{
#if !SILVERLIGHT
    [Serializable]
#endif
    public sealed class JsonRpcException : Exception
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
