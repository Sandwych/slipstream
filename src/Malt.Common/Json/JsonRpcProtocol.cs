using System;
using System.Collections.Generic;
using System.Linq;

namespace Malt.Json
{
    public static class JsonRpcProtocol
    {
        public const string JsonContentType = "text/json";
        public const string Method = "method";
        public const string Id = "id";
        public const string Params = "params";
        public const string Result = "result";
    }
}
