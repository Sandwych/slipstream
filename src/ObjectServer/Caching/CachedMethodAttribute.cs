using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class CachedMethodAttribute : Attribute
    {
        public CachedMethodAttribute()
        {
            this.Timeout = 1200;
        }

        /// <summary>
        /// timeout in secs
        /// </summary>
        public int Timeout { get; set; }
    }
}
