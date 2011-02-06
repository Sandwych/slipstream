using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class ServiceMethodAttribute : Attribute
    {
    }
}
