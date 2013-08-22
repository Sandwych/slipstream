using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SlipStream
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class ResourceAttribute : Attribute
    {
        public ResourceAttribute()
        {
        }
    }
}
