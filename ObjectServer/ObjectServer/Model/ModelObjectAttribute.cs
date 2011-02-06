using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class ModelObjectAttribute : Attribute
    {

    }
}
