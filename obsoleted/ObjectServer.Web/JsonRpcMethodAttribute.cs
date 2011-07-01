using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ObjectServer.Web
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class JsonRpcMethodAttribute : Attribute
    {
        public JsonRpcMethodAttribute()
        {
        }

        public string Name { get; set; }
    }
}
